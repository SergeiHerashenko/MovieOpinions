using Authorization.Application.DTOs.Communication;
using Authorization.Application.Interfaces.Communication;
using Authorization.Domain.Results;
using Authorization.Infrastructure.Errors.Integration;
using Authorization.Infrastructure.Http;
using Authorization.Infrastructure.Http.Models;
using Authorization.Infrastructure.Http.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authorization.Infrastructure.Integration
{
    public class NotificationSender : INotificationSender
    {
        private readonly NotificationServiceOptions _options;
        private readonly ILogger<NotificationSender> _logger;
        private readonly ISendInternalRequest _sendInternalRequest;

        public NotificationSender(
            IOptions<NotificationServiceOptions> options,
            ILogger<NotificationSender> logger,
            ISendInternalRequest sendInternalRequest)
        {
            _options = options.Value;
            _logger = logger;
            _sendInternalRequest = sendInternalRequest;
        }

        public async Task<Result> SendCreateNotificationAsync(NotificationCommand notificationCommand)
        {
            var notificationRequest = new InternalRequest<NotificationCommand>
            {
                ClientName = _options.ClientName,
                Endpoint = _options.CreateEndpoint,
                Method = HttpMethod.Post,
                Body = notificationCommand
            };

            var responseNotification = await _sendInternalRequest.SendAsync<NotificationCommand, bool>(notificationRequest);

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
