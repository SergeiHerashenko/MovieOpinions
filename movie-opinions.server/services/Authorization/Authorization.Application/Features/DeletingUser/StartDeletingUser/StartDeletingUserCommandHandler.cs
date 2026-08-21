using Authorization.Application.Abstractions.AggregateChanges;
using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.RateLimiter;
using Authorization.Application.Abstractions.Security.Hashers;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Errors.Users;
using Authorization.Application.DTOs.Communication.Contacts.Requests;
using Authorization.Application.DTOs.Communication.Contacts.Responses;
using Authorization.Application.Features.DeletingUser.StartDeletingUser.Model;
using Authorization.Domain.Results;
using Authorization.Domain.Users;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.PasswordUser;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Features.DeletingUser.StartDeletingUser
{
    public class StartDeletingUserCommandHandler : IRequestHandler<StartDeletingUserCommand, Result<StartDeletingUserResult>>
    {
        private readonly IRateLimiter _rateLimiter;
        private readonly IUserContext _userContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IClock _clock;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        private readonly IContactsSender _contactsSender;

        private readonly ILogger<StartDeletingUserCommandHandler> _logger;

        private readonly IAggregateChangesDispatcher _aggregateChangesDispatcher;

        public StartDeletingUserCommandHandler(
            IRateLimiter rateLimiter,
            IUserContext userContext,
            IPasswordHasher passwordHasher,
            IClock clock,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IContactsSender contactsSender,
            ILogger<StartDeletingUserCommandHandler> logger,
            IAggregateChangesDispatcher aggregateChangesDispatcher)
        {
            _rateLimiter = rateLimiter;
            _userContext = userContext;
            _passwordHasher = passwordHasher;
            _clock = clock;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _contactsSender = contactsSender;
            _logger = logger;
            _aggregateChangesDispatcher = aggregateChangesDispatcher;
        }

        public async Task<Result<StartDeletingUserResult>> Handle(StartDeletingUserCommand command, CancellationToken cancellationToken)
        {
            var ipAddressResult = IpAddress.Create(_userContext.GetIpAddress());

            if (ipAddressResult.IsFailure)
                return Result<StartDeletingUserResult>.Failure(ipAddressResult.Errors);

            var userId = UserId.Restore(_userContext.GetUserId());

            var limiterResult = await _rateLimiter.EnsureAllowedAsync(
                RateLimitAction.ChangeDeletingUser,
                ipAddressResult.Value,
                userId.Value.ToString(),
                cancellationToken
            );

            if (limiterResult.IsFailure)
                return Result<StartDeletingUserResult>.Failure(limiterResult.Errors);

            cancellationToken.ThrowIfCancellationRequested();

            var passwordResult = await VerifyCurrentPasswordAsync(userId, command.Password, cancellationToken);

            if (passwordResult.IsFailure)
                return Result<StartDeletingUserResult>.Failure(passwordResult.Errors);

            cancellationToken.ThrowIfCancellationRequested();

            var startResult = await StartDeletionAsync(userId, command.Reason, cancellationToken);

            if (startResult.IsFailure)
                return Result<StartDeletingUserResult>.Failure(startResult.Errors);

            var startedDeletion = startResult.Value;

            var channelsResult = await GetAvailableChannelsAsync(startedDeletion, cancellationToken);

            if (channelsResult.IsFailure)
                return Result<StartDeletingUserResult>.Failure(channelsResult.Errors);

            var response = new StartDeletingUserResult(startedDeletion.Action.ConfirmationToken, channelsResult.Value);

            return Result<StartDeletingUserResult>.Success(response);
        }

        private async Task<Result> VerifyCurrentPasswordAsync(UserId userId, string rawPassword, CancellationToken cancellationToken = default)
        {
            var plainPasswordResult = PlainPassword.Create(rawPassword);

            if (plainPasswordResult.IsFailure)
                return Result.Failure(plainPasswordResult.Errors);

            var existingUser = await _userRepository.GetUserByIdAsync(userId, cancellationToken);

            if (existingUser is null)
            {
                _passwordHasher.FakeVerifyPassword(plainPasswordResult.Value);

                return Result.Failure(UserErrors.NotFound<StartDeletingUserCommandHandler>(userId.Value.ToString()));
            }

            var passwordIsValid = _passwordHasher.VerifyPassword(plainPasswordResult.Value, existingUser.Password.Value);

            if (passwordIsValid)
                return Result.Success();

            existingUser.RecordFailedPasswordAttempt(_clock.UtcNow);

            await _unitOfWork.ExecuteAsync(async ct =>
            {
                await _userRepository.UpdateUserAsync(existingUser, ct);
            }, cancellationToken);

            return Result.Failure(UserErrors.InvalidPassword<StartDeletingUserCommandHandler>(userId.Value.ToString()));
        }

        private async Task<Result<StartedDeletion>> StartDeletionAsync(UserId userId, string? reason, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ExecuteAsync(async ct =>
            {
                var currentUser = await _userRepository.GetUserByIdForUpdateAsync(userId, ct);

                if (currentUser is null)
                    return Result<StartedDeletion>.Failure(UserErrors.NotFound<StartDeletingUserCommandHandler>(userId.Value.ToString()));

                var deletion = currentUser.GetDeletion();

                if (deletion.IsSuccess)
                    return Result<StartedDeletion>.Failure(UserErrors.UserIsDeleted<StartDeletingUserCommandHandler>(deletion.Value.RestoreUntil));

                var actionResult = currentUser.ActionDeletingUser(reason, _clock.UtcNow);

                if(actionResult.IsFailure)
                    return Result<StartedDeletion>.Failure(actionResult.Errors);

                await _aggregateChangesDispatcher.DispatchAsync(currentUser.AggregateChanges, ct);

                return Result<StartedDeletion>.Success(
                    new StartedDeletion(
                        currentUser, 
                        actionResult.Value
                    )
                );
            }, cancellationToken);

            if (result.IsSuccess)
                result.Value.User.ClearAggregateChanges();

            return result;
        }

        private async Task<Result<IReadOnlyCollection<AvailableDeletionChannel>>> GetAvailableChannelsAsync(
            StartedDeletion startedDeletion,
            CancellationToken cancellationToken = default)
        {
            //var request = ActiveChannelsRequest.Create(startedDeletion.User.Id);
            //
            //var contactsResult = await _contactsSender.GetActiveContactsAsync(request, cancellationToken);
            //
            //if (contactsResult.IsFailure)
            //{
            //    var failResult = await MarkPendingActionAsFailedAsync(
            //        startedDeletion.User,
            //        startedDeletion.Action.Id,
            //        cancellationToken
            //    );
            //
            //    if (failResult.IsFailure)
            //    {
            //        _logger.LogCritical(
            //            "Failed to compensate pending deletion action {ActionId} for user {UserId}!",
            //            startedDeletion.Action.Id.Value,
            //            startedDeletion.User.Id.Value
            //        );
            //
            //        return Result<IReadOnlyCollection<AvailableDeletionChannel>>.Failure(failResult.Errors);
            //    }
            //
            //    return Result<IReadOnlyCollection<AvailableDeletionChannel>>.Failure(contactsResult.Errors);
            //}
            //
            //var channels = contactsResult.Value.Channels;
            //
            //if (channels.Count == 0)
            //{
            //    _logger.LogError(
            //        "User {UserId} has no active verified contact channels. The user-contact invariant is violated!",
            //        startedDeletion.User.Id.Value
            //    );
            //
            //    var failResult = await MarkPendingActionAsFailedAsync(
            //        startedDeletion.User,
            //        startedDeletion.Action.Id,
            //        cancellationToken
            //    );
            //
            //    if (failResult.IsFailure)
            //    {
            //        _logger.LogCritical(
            //            "Failed to compensate pending deletion action {ActionId} for user {UserId}!",
            //            startedDeletion.Action.Id.Value,
            //            startedDeletion.User.Id.Value
            //        );
            //
            //        return Result<IReadOnlyCollection<AvailableDeletionChannel>>.Failure(failResult.Errors);
            //    }
            //
            //    return Result<IReadOnlyCollection<AvailableDeletionChannel>>.Failure(UserErrors.NoVerifiedContactChannels<StartDeletingUserCommandHandler>(startedDeletion.User.Login.Value));
            //}

            IReadOnlyCollection<ActiveContactChannelResponse> channels = new List<ActiveContactChannelResponse>
            {
                new ActiveContactChannelResponse(
                    Guid.NewGuid(),
                    CommunicationChannel.Email, 
                    "s*****@gmail.com"
                ),
                new ActiveContactChannelResponse(
                    Guid.NewGuid(),
                    CommunicationChannel.Phone,
                    "+380****5646"
                )
            };

            var availableChannels = channels
                .Select(channel => new AvailableDeletionChannel(
                    channel.ContactId,
                    channel.Channel,
                    channel.MaskedValue))
                .ToArray();

            return Result<IReadOnlyCollection<AvailableDeletionChannel>>.Success(availableChannels);
        }

        private async Task<Result> MarkPendingActionAsFailedAsync(
            User user,
            UserPendingActionId actionId,
            CancellationToken cancellationToken = default)
        {
            var failResult = user.FailPendingAction(actionId, _clock.UtcNow);

            if (failResult.IsFailure)
                return failResult;

            await _unitOfWork.ExecuteAsync(async ct =>
            {
                await _aggregateChangesDispatcher.DispatchAsync(user.AggregateChanges, ct);
            }, cancellationToken);

            user.ClearAggregateChanges();

            return Result.Success();
        }
    }
}
