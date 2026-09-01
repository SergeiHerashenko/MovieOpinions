using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Abstractions.Mapping;
using Authorization.Application.Abstractions.RateLimiter;
using Authorization.Application.Abstractions.Services.UserActionConfirmation;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Application.DTOs.Communication.Verification;
using Authorization.Application.Features.Services.UserActionConfirmation.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Application.Features.Services.UserActionConfirmation
{
    public sealed class VerifyUserActionConfirmationService : IVerifyUserActionConfirmationService
    {
        private readonly IUserPendingActionRetriever _userPendingActionRetriever;
        private readonly IUserActionConfirmationMapping _userActionConfirmationMapping;
        private readonly IUserContext _userContext;
        private readonly IRateLimiter _rateLimiter;
        private readonly IVerificationSender _verificationSender;

        public VerifyUserActionConfirmationService(
            IUserPendingActionRetriever userPendingActionRetriever,
            IUserActionConfirmationMapping userActionConfirmationMapping,
            IUserContext userContext,
            IRateLimiter rateLimiter,
            IVerificationSender verificationSender)
        {
            _userPendingActionRetriever = userPendingActionRetriever;
            _userActionConfirmationMapping = userActionConfirmationMapping;
            _userContext = userContext;
            _rateLimiter = rateLimiter;
            _verificationSender = verificationSender;
        }

        public async Task<Result<VerifiedUserActionResult>> VerifyAsync<TAction>(
            VerifiedUserActionData data, 
            CancellationToken cancellationToken = default) 
            where TAction : UserAction
        {
            var userId = UserId.Restore(_userContext.GetUserId());
            var ipAddressResult = IpAddress.Create(_userContext.GetIpAddress());

            if (ipAddressResult.IsFailure)
                return Result<VerifiedUserActionResult>.Failure(ipAddressResult.Errors);

            var verificationConfig = _userActionConfirmationMapping.GetVerificationConfiguration<TAction>();

            var limiterResult = await _rateLimiter.EnsureAllowedAsync(
                verificationConfig.RateLimitAction,
                ipAddressResult.Value,
                userId.Value.ToString(),
                cancellationToken
            );

            if (limiterResult.IsFailure)
                return Result<VerifiedUserActionResult>.Failure(limiterResult.Errors);

            cancellationToken.ThrowIfCancellationRequested();

            var activAction = await _userPendingActionRetriever.RetrieverAsync<TAction>(
                userId,
                data.ConfirmationToken,
                cancellationToken
            );

            if (activAction.IsFailure)
                return Result<VerifiedUserActionResult>.Failure(activAction.Errors);

            var verificationCommand = VerificationRequest.Create(
                activAction.Value.Id,
                verificationConfig.VerificationType,
                data.VerificationValue
            );
            
            var resultVerify = await _verificationSender.VerifyCodeAsync(verificationCommand, cancellationToken);
            
            if (resultVerify.IsFailure)
                return Result<VerifiedUserActionResult>.Failure(resultVerify.Errors);

            var result = new VerifiedUserActionResult(userId, activAction.Value.Id);

            return Result<VerifiedUserActionResult>.Success(result);
        }
    }
}
