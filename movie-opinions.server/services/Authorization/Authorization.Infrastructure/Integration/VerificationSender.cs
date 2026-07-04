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
    public class VerificationSender : IVerificationSender
    {
        private readonly VerificationServiceOptions _options;
        private readonly ILogger<VerificationSender> _logger;
        private readonly ISendInternalRequest _sendInternalRequest;

        public VerificationSender(
            IOptions<VerificationServiceOptions> options,
            ILogger<VerificationSender> logger,
            ISendInternalRequest sendInternalRequest)
        {
            _options = options.Value;
            _logger = logger;
            _sendInternalRequest = sendInternalRequest;
        }

        public async Task<Result> VerifyCodeAsync(VerificationCommand verificationCommand)
        {
            var verificationRequest = new InternalRequest<VerificationCommand>
            {
                ClientName = _options.ClientName,
                Endpoint = _options.CreateEndpoint,
                Method = HttpMethod.Post,
                Body = verificationCommand
            };

            var responseVerification = await _sendInternalRequest.SendAsync<VerificationCommand, bool>(verificationRequest);

            if (!responseVerification.IsSuccess)
            {
                _logger.LogError("Failed to send verification request to {ClientName} ({Endpoint}). User: {UserId}, Action: {Action}!",
                    _options.ClientName,
                    _options.CreateEndpoint,
                    verificationCommand.UserId.Value, 
                    verificationCommand.MessageActions
                );

                return Result.Failure(CommunicationError.SendError<VerificationSender>(
                    $"Failed to perform operation verification ({verificationCommand.MessageActions}) for user!")
                );
            }

            return Result.Success();
        }
    }
}
