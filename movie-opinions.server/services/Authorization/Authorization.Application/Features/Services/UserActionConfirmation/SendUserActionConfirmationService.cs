using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Abstractions.Mapping;
using Authorization.Application.Abstractions.RateLimiter;
using Authorization.Application.Abstractions.Services.UserActionConfirmation;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Application.Common.Enums;
using Authorization.Application.DTOs.Communication;
using Authorization.Application.DTOs.Communication.Contacts.Requests;
using Authorization.Application.Features.Services.UserActionConfirmation.Models;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Application.Features.Services.UserActionConfirmation
{
    public sealed class SendUserActionConfirmationService : ISendUserActionConfirmationService
    {
        private readonly IUserPendingActionRetriever _userPendingActionRetriever;
        private readonly IUserActionConfirmationMapping _userActionConfirmationMapping;
        private readonly IUserContext _userContext;
        private readonly IRateLimiter _rateLimiter;
        private readonly IContactsSender _contactsSender;
        private readonly INotificationSender _notificationSender;

        public SendUserActionConfirmationService(
            IUserPendingActionRetriever userPendingActionRetriever,
            IUserActionConfirmationMapping userActionConfirmationMapping,
            IUserContext userContext,
            IRateLimiter rateLimiter,
            IContactsSender contactsSender,
            INotificationSender notificationSender)
        {
            _userPendingActionRetriever = userPendingActionRetriever;
            _userActionConfirmationMapping = userActionConfirmationMapping;
            _userContext = userContext;
            _rateLimiter = rateLimiter;
            _contactsSender = contactsSender;
            _notificationSender = notificationSender;
        }

        public async Task<Result<SendActionConfirmationResult>> SendAsync<TAction>(
            SendActionConfirmationData data, 
            CancellationToken cancellationToken = default) 
            where TAction : UserAction
        {
            if (!Guid.TryParse(data.ContactId, out Guid contactId))
                return Result<SendActionConfirmationResult>.Failure(ActionErrors.InvalidInputData<SendUserActionConfirmationService>());

            var userId = UserId.Restore(_userContext.GetUserId());
            var ipAddressResult = IpAddress.Create(_userContext.GetIpAddress());

            if (ipAddressResult.IsFailure)
                return Result<SendActionConfirmationResult>.Failure(ipAddressResult.Errors);

            var notificationConfig = _userActionConfirmationMapping.GetNotificationConfiguration<TAction>();

            var limiterResult = await _rateLimiter.EnsureAllowedAsync(
                notificationConfig.RateLimitAction,
                ipAddressResult.Value,
                userId.Value.ToString(),
                cancellationToken
            );

            if (limiterResult.IsFailure)
                return Result<SendActionConfirmationResult>.Failure(limiterResult.Errors);

            cancellationToken.ThrowIfCancellationRequested();

            var activAction = await _userPendingActionRetriever.RetrieverAsync<TAction>(
                userId,
                data.ConfirmationToken,
                cancellationToken
            );

            if (activAction.IsFailure)
                return Result<SendActionConfirmationResult>.Failure(activAction.Errors);

            var contactRequest = ActiveContactRequest.Create(userId, contactId);

            var contactResult = await _contactsSender.GetActiveContactAsync(contactRequest, cancellationToken);

            if (contactResult.IsFailure)
                return Result<SendActionConfirmationResult>.Failure(contactResult.Errors);

            var sendNotificationRequest = NotificationRequest.Create(
                activAction.Value.Id,
                contactResult.Value.ContactValue,
                notificationConfig.NotificationType,
                contactResult.Value.Channel
            );

            await _notificationSender.SendCreateNotificationAsync(sendNotificationRequest, cancellationToken);

            var message = CreateMessage(contactResult.Value.Channel, data.MaskedValue);

            var nextStep = contactResult.Value.Channel == CommunicationChannel.Email
                ? ConfirmationNextStep.EmailNextStep
                : ConfirmationNextStep.PhoneNextStep;

            var result = new SendActionConfirmationResult(nextStep, message);

            return Result<SendActionConfirmationResult>.Success(result);
        }

        private string CreateMessage(
            CommunicationChannel channel, string maskedValue)
        {
            return channel switch
            {
                CommunicationChannel.Email => $"An email has been sent to your email address {maskedValue}. Please follow the rules in the email!",
                _ => $"A code has been sent to your {channel} ({maskedValue}). Please enter it in the fields below!"
            };
        }
    }
}
