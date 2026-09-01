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

namespace Authorization.Application.Features.DeletingUser.ConfirmationDeleting
{
    public class ConfirmationDeletingHandler 
        : IRequestHandler<ConfirmationDeletingCommand, Result>
    {
        private readonly IClock _clock;

        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IVerifyUserActionConfirmationService _verifyUserActionConfirmationService;

        private readonly IAggregateChangesDispatcher _aggregateChangesDispatcher;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public ConfirmationDeletingHandler(
            IClock clock,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IVerifyUserActionConfirmationService verifyUserActionConfirmationService,
            IAggregateChangesDispatcher aggregateChangesDispatcher,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _clock = clock;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _verifyUserActionConfirmationService = verifyUserActionConfirmationService;
            _aggregateChangesDispatcher = aggregateChangesDispatcher;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Result> Handle(ConfirmationDeletingCommand command, CancellationToken cancellationToken = default)
        {
            var verifyData = new VerifiedUserActionData(
                command.ConfirmationToken,
                command.VerificationValue
            );

            var verificationResult = await _verifyUserActionConfirmationService.VerifyAsync<DeleteAccountAction>(
                verifyData,
                cancellationToken
            );

            if (verificationResult.IsFailure)
                return Result.Failure(verificationResult.Errors);

            var resultDeletion = await _unitOfWork.ExecuteAsync(async ct =>
            {
                var currentUser = await _userRepository.GetUserByIdForUpdateAsync(verificationResult.Value.UserId, ct);

                if (currentUser is null)
                    return Result<User>.Failure(UserErrors.NotFound<ConfirmationDeletingHandler>(verificationResult.Value.UserId.Value.ToString()));

                var deletionResult = currentUser.ConfirmDeleting(
                    command.ConfirmationToken,
                    verificationResult.Value.ActionId, 
                    _clock.UtcNow
                );

                if (deletionResult.IsFailure)
                    return Result<User>.Failure(deletionResult.Errors);

                foreach (var refreshToken in currentUser.RefreshTokens.ToList())
                {
                    var revokeToken = currentUser.RevokeRefreshToken(refreshToken.Id, _clock.UtcNow);

                    if(revokeToken.IsFailure)
                        return Result<User>.Failure(revokeToken.Errors);
                }

                await _aggregateChangesDispatcher.DispatchAsync(currentUser.AggregateChanges, ct);
                
                currentUser.ClearAggregateChanges();

                return Result<User>.Success(currentUser);

            }, cancellationToken);

            if(resultDeletion.IsFailure)
                return Result.Failure(resultDeletion.Errors);

            var committedUser = resultDeletion.Value;

            await _domainEventDispatcher.DispatchAsync(committedUser.DomainEvents, cancellationToken);

            committedUser.ClearDomainEvents();

            return Result.Success();
        }
    }
}
