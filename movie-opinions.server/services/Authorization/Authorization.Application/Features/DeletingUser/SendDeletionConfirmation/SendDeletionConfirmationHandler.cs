using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.RateLimiter;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Errors.Users;
using Authorization.Application.DTOs.Communication;
using Authorization.Application.DTOs.Communication.Contacts.Requests;
using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.ValueObjects;
using MediatR;

namespace Authorization.Application.Features.DeletingUser.SendDeletionConfirmation
{
    public class SendDeletionConfirmationHandler : IRequestHandler<SendDeletionConfirmationCommand, Result<SendDeletionConfirmationResult>>
    {
        private readonly IUserContext _userContext;
        private readonly IRateLimiter _rateLimiter;
        private readonly IClock _clock;

        private readonly IUserRepository _userRepository;

        private readonly IContactsSender _contactsSender;
        private readonly INotificationSender _notificationSender;

        public SendDeletionConfirmationHandler(
            IUserContext userContext,
            IRateLimiter rateLimiter,
            IClock clock,
            IUserRepository userRepository,
            IContactsSender contactsSender,
            INotificationSender notificationSender)
        {
            _userContext = userContext;
            _rateLimiter = rateLimiter;
            _clock = clock;
            _userRepository = userRepository;
            _contactsSender = contactsSender;
            _notificationSender = notificationSender;
        }

        public async Task<Result<SendDeletionConfirmationResult>> Handle(SendDeletionConfirmationCommand command, CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(command.ContactId, out Guid contactId))
                return Result<SendDeletionConfirmationResult>.Failure(ActionErrors.InvalidInputData<SendDeletionConfirmationHandler>());

            var userId = UserId.Restore(_userContext.GetUserId());
            var ipAddressResult = IpAddress.Create(_userContext.GetIpAddress());

            if (ipAddressResult.IsFailure)
                return Result<SendDeletionConfirmationResult>.Failure(ipAddressResult.Errors);

            var limiterResult = await _rateLimiter.EnsureAllowedAsync(
                RateLimitAction.SendChangeDeletionConfirmation,
                ipAddressResult.Value,
                userId.Value.ToString(),
                cancellationToken
            );

            if (limiterResult.IsFailure)
                return Result<SendDeletionConfirmationResult>.Failure(limiterResult.Errors);

            cancellationToken.ThrowIfCancellationRequested();

            var existingUser = await _userRepository.GetUserByIdAsync(userId, cancellationToken);

            if (existingUser is null)
                return Result<SendDeletionConfirmationResult>.Failure(UserErrors.NotFound<SendDeletionConfirmationHandler>(userId.Value.ToString()));

            var confirmationToken = ConfirmationToken.Restore(command.ConfirmationToken);

            var actionResult = existingUser.GetDeletionActionForConfirmation(confirmationToken, _clock.UtcNow);

            if (actionResult.IsFailure)
                return Result<SendDeletionConfirmationResult>.Failure(actionResult.Errors);

            var contactRequest = ActiveContactRequest.Create(userId, contactId);

            var contactResult = await _contactsSender.GetActiveContactAsync(contactRequest, cancellationToken);

            if (contactResult.IsFailure)
                return Result<SendDeletionConfirmationResult>.Failure(contactResult.Errors);

            var sendNotificationRequest = NotificationRequest.Create(
                actionResult.Value.Id,
                contactResult.Value.ContactValue,
                NotificationType.ActionDeletionUser,
                contactResult.Value.Channel
            );

            var sendNotification = await _notificationSender.SendCreateNotificationAsync(sendNotificationRequest, cancellationToken);

            var message = CreateMessage(contactResult.Value.Channel, command.MaskedValue);

            var nextStep = contactResult.Value.Channel == CommunicationChannel.Email
                ? ConfirmationNextStep.EmailNextStep
                : ConfirmationNextStep.PhoneNextStep;

            var result = new SendDeletionConfirmationResult(nextStep, message);

            return Result<SendDeletionConfirmationResult>.Success(result);
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
