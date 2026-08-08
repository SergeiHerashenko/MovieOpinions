using Authorization.Application.Abstractions.Communication;
using Authorization.Application.DTOs.Communication;
using Authorization.Domain.Results;
using Authorization.Infrastructure.Communication.Options;
using Authorization.Infrastructure.Communication.SenderPermissions;
using Authorization.Infrastructure.Errors.Communication;
using Authorization.Infrastructure.Http;
using Authorization.Infrastructure.Http.Models;
using Authorization.Infrastructure.Security.JWT.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Authorization.Infrastructure.Communication
{
    public class VerificationSender : IVerificationSender
    {
        private readonly VerificationServiceOptions _options;
        private readonly ServiceIdentityOptions _identityOptions;
        private readonly ILogger<VerificationSender> _logger;
        private readonly ISendInternalRequest _sendInternalRequest;
        private readonly IServiceJwtProvider _serviceJwtProvider;

        public VerificationSender(
            IOptions<VerificationServiceOptions> options,
            IOptions<ServiceIdentityOptions> identityOptions,
            ILogger<VerificationSender> logger,
            ISendInternalRequest sendInternalRequest,
            IServiceJwtProvider serviceJwtProvider)
        {
            _options = options.Value;
            _identityOptions = identityOptions.Value;
            _logger = logger;
            _sendInternalRequest = sendInternalRequest;
            _serviceJwtProvider = serviceJwtProvider;
        }

        public async Task<Result> VerifyCodeAsync<TId>(VerificationRequest<TId> verificationCommand)
        {
            var token = _serviceJwtProvider.GenerateServiceToken(_identityOptions.ServiceName, new[] { Permissions.Verification.Audit });

            var verificationRequest = new InternalRequest<VerificationRequest<TId>>
            {
                ClientName = _options.ClientName,
                Endpoint = _options.CreateEndpoint,
                Method = HttpMethod.Post,
                Body = verificationCommand,
                Headers = new Dictionary<string, string>()
                {
                    { _identityOptions.HeaderName, $"{_identityOptions.Scheme} {token}" }
                }
            };

            var responseVerification = await _sendInternalRequest.SendAsync<VerificationRequest<TId>, bool>(verificationRequest);

            if (!responseVerification.IsSuccess)
            {
                _logger.LogError("Failed to send verification request to {ClientName} ({Endpoint}). User: {UserId}, Action: {Action}!",
                    _options.ClientName,
                    _options.CreateEndpoint,
                    verificationCommand.UserId,
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
