using Authorization.Application.DTOs.Communication;
using Authorization.Application.Interfaces.Communication;
using Authorization.Domain.Results;
using Authorization.Infrastructure.Errors.Integration;
using Authorization.Infrastructure.Http;
using Authorization.Infrastructure.Http.Models;
using Authorization.Infrastructure.Integration.Options;
using Authorization.Infrastructure.Integration.SenderPermissions;
using Authorization.Infrastructure.Security.JWT.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authorization.Infrastructure.Integration
{
    public class NotificationSender : INotificationSender
    {
        private readonly NotificationServiceOptions _options;
        private readonly ServiceIdentityOptions _identityOptions;
        private readonly ILogger<NotificationSender> _logger;
        private readonly ISendInternalRequest _sendInternalRequest;
        private readonly IServiceJwtProvider _serviceJwtProvider;

        public NotificationSender(
            IOptions<NotificationServiceOptions> options,
            IOptions<ServiceIdentityOptions> identityOptions,
            ILogger<NotificationSender> logger,
            ISendInternalRequest sendInternalRequest,
            IServiceJwtProvider serviceJwtProvider)
        {
            _options = options.Value;
            _identityOptions = identityOptions.Value;
            _logger = logger;
            _sendInternalRequest = sendInternalRequest;
            _serviceJwtProvider = serviceJwtProvider;
        }

        public async Task<Result> SendCreateNotificationAsync<TId>(NotificationRequest<TId> notificationCommand)
        {
            var token = _serviceJwtProvider.GenerateServiceToken(_identityOptions.ServiceName, new[] { Permissions.Notification.Create });

            var notificationRequest = new InternalRequest<NotificationRequest<TId>>
            {
                ClientName = _options.ClientName,
                Endpoint = _options.CreateEndpoint,
                Method = HttpMethod.Post,
                Body = notificationCommand,
                Headers = new Dictionary<string, string>()
                {
                    { _identityOptions.HeaderName, $"{_identityOptions.Scheme} {token}" }
                }
            };

            var responseNotification = await _sendInternalRequest.SendAsync<NotificationRequest<TId>, bool>(notificationRequest);

            if (!responseNotification.IsSuccess)
            {
                _logger.LogError("Notification sending failed after retries. Registration will proceed without blocking. Recipient: {Recipient}, Action: {Action}, Reason: {Reason}",
                    notificationCommand.Recipient,
                    notificationCommand.Action,
                    responseNotification.ErrorMessage
                );

                return Result.Failure(CommunicationError.SendError<NotificationSender>("Error sending confirmation email"));
            }

            return Result.Success();
        }
    }
}
