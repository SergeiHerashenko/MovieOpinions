using Authorization.Application.Abstractions.AggregateChanges;
using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Events;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.Services.UserActionConfirmation;
using Authorization.Application.Common.Errors.Users;
using Authorization.Application.Features.Services.UserActionConfirmation.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;
using MediatR;

namespace Authorization.Application.Features.ChangingPassword.ConfirmationChangePassword
{
    public class ConfirmationChangePasswordHandler
        : IRequestHandler<ConfirmationChangePasswordCommand, Result>
    {
        private readonly IVerifyUserActionConfirmationService _verifyUserActionConfirmationService;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IClock _clock;
        private readonly IAggregateChangesDispatcher _aggregateChangesDispatcher;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public ConfirmationChangePasswordHandler(
            IVerifyUserActionConfirmationService verifyUserActionConfirmationService,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IClock clock,
            IAggregateChangesDispatcher aggregateChangesDispatcher,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _verifyUserActionConfirmationService = verifyUserActionConfirmationService;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _clock = clock;
            _aggregateChangesDispatcher = aggregateChangesDispatcher;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Result> Handle(
            ConfirmationChangePasswordCommand command, 
            CancellationToken cancellationToken = default)
        {
            var verifyData = new VerifiedUserActionData(
                command.ConfirmationToken,
                command.VerificationValue
            );

            var verificationResult = await _verifyUserActionConfirmationService.VerifyAsync<PasswordChangeAction>(
                verifyData,
                cancellationToken
            );

            if (verificationResult.IsFailure)
                return Result.Failure(verificationResult.Errors);

            var resultChange = await _unitOfWork.ExecuteAsync(async ct =>
            {
                var currentUser = await _userRepository.GetUserByIdForUpdateAsync(verificationResult.Value.UserId, ct);

                if (currentUser is null)
                    return Result<User>.Failure(UserErrors.NotFound<ConfirmationChangePasswordHandler>(verificationResult.Value.UserId.Value.ToString()));

                var changePasswordResult = currentUser.ConfirmPassword(
                    command.ConfirmationToken,
                    verificationResult.Value.ActionId,
                    _clock.UtcNow
                );

                if (changePasswordResult.IsFailure)
                    return Result<User>.Failure(changePasswordResult.Errors);

                foreach(var refreshToken in currentUser.RefreshTokens.ToList())
                {
                    var revokeToken = currentUser.RevokeRefreshToken(refreshToken.Id, _clock.UtcNow);

                    return Result<User>.Failure(revokeToken.Errors);
                }

                await _aggregateChangesDispatcher.DispatchAsync(currentUser.AggregateChanges, ct);

                currentUser.ClearAggregateChanges();

                return Result<User>.Success(currentUser);

            }, cancellationToken);

            if (resultChange.IsFailure)
                return Result.Failure(resultChange.Errors);

            var committedUser = resultChange.Value;

            await _domainEventDispatcher.DispatchAsync(committedUser.DomainEvents, cancellationToken);

            committedUser.ClearDomainEvents();

            return Result.Success();
        }
    }
}
