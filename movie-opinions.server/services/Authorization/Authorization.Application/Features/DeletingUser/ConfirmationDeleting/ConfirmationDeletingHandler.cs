using Authorization.Application.Abstractions.AggregateChanges;
using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Abstractions.Events;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.RateLimiter;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Errors.Users;
using Authorization.Application.DTOs.Communication.Verification;
using Authorization.Application.DTOs.Communication.Verification.Enums;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.ValueObjects;
using MediatR;

namespace Authorization.Application.Features.DeletingUser.ConfirmationDeleting
{
    public class ConfirmationDeletingHandler : IRequestHandler<ConfirmationDeletingCommand, Result>
    {
        private readonly IUserContext _userContext;
        private readonly IRateLimiter _rateLimiter;
        private readonly IClock _clock;

        private readonly IUserRepository _userRepository;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IVerificationSender _verificationSender;

        private readonly IAggregateChangesDispatcher _aggregateChangesDispatcher;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public ConfirmationDeletingHandler(
            IUserContext userContext,
            IRateLimiter rateLimiter,
            IClock clock,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IVerificationSender verificationSender,
            IAggregateChangesDispatcher aggregateChangesDispatcher,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _userContext = userContext;
            _rateLimiter = rateLimiter;
            _clock = clock;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _verificationSender = verificationSender;
            _aggregateChangesDispatcher = aggregateChangesDispatcher;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Result> Handle(ConfirmationDeletingCommand command, CancellationToken cancellationToken = default)
        {
            var userId = UserId.Restore(_userContext.GetUserId());
            var ipAddressResult = IpAddress.Create(_userContext.GetIpAddress());

            if (ipAddressResult.IsFailure)
                return Result.Failure(ipAddressResult.Errors);

            var limiterResult = await _rateLimiter.EnsureAllowedAsync(
                RateLimitAction.ConfirmationDeletingUser,
                ipAddressResult.Value,
                userId.Value.ToString(),
                cancellationToken
            );

            if(limiterResult.IsFailure)
                return Result.Failure(limiterResult.Errors);

            cancellationToken.ThrowIfCancellationRequested();

            var existingUser = await _userRepository.GetUserByIdAsync(userId, cancellationToken);

            if (existingUser is null)
                return Result.Failure(UserErrors.NotFound<ConfirmationDeletingHandler>(userId.Value.ToString()));

            var confirmationToken = ConfirmationToken.Restore(command.ConfirmationToken);

            var actionResult = existingUser.GetDeletionActionForConfirmation(confirmationToken, _clock.UtcNow);

            if (actionResult.IsFailure)
                return Result.Failure(actionResult.Errors);

            //var verificationCommand = VerificationRequest.Create(
            //    actionResult.Value.Id,
            //    VerificationType.DeletionUser,
            //    command.VerificationValue
            //);
            //
            //var result = await _verificationSender.VerifyCodeAsync(verificationCommand, cancellationToken);
            //
            //if (result.IsFailure)
            //    return Result.Failure(result.Errors);

            var resultDeletion = await _unitOfWork.ExecuteAsync(async ct =>
            {
                var currentUser = await _userRepository.GetUserByIdForUpdateAsync(userId, ct);

                if (currentUser is null)
                    return Result.Failure(UserErrors.NotFound<ConfirmationDeletingHandler>(userId.Value.ToString()));

                var pendingAction = currentUser.GetPendingAction();

                if (pendingAction.IsFailure)
                    return Result.Failure(pendingAction.Errors);

                var deletionResult = currentUser.ConfirmDeleting(command.ConfirmationToken, _clock.UtcNow);

                if (deletionResult.IsFailure)
                    return Result.Failure(deletionResult.Errors);

                await _aggregateChangesDispatcher.DispatchAsync(currentUser.AggregateChanges, ct);
                await _domainEventDispatcher.DispatchAsync(currentUser.DomainEvents, ct);

                currentUser.ClearAggregateChanges();
                currentUser.ClearDomainEvents();

                return Result.Success();

            }, cancellationToken);

            if(resultDeletion.IsFailure)
                return Result.Failure(resultDeletion.Errors);

            return Result.Success();
        }
    }
}
