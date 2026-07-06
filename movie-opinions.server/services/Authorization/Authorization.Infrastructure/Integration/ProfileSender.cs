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
    public class ProfileSender : IProfileSender
    {
        private readonly ProfileServiceOption _options;
        private readonly ServiceIdentityOptions _identityOptions;
        private readonly ILogger<ProfileSender> _logger;
        private readonly ISendInternalRequest _sendInternalRequest;
        private readonly IServiceJwtProvider _serviceJwtProvider;

        public ProfileSender(
            IOptions<ProfileServiceOption> options,
            IOptions<ServiceIdentityOptions> identityOptions,
            ILogger<ProfileSender> logger,
            ISendInternalRequest sendInternalRequest,
            IServiceJwtProvider serviceJwtProvider)
        {
            _options = options.Value;
            _identityOptions = identityOptions.Value;
            _logger = logger;
            _sendInternalRequest = sendInternalRequest;
            _serviceJwtProvider = serviceJwtProvider;
        }

        public async Task<Result> SendCreateProfileRequestAsync<TId>(ProfileRequest<TId> profileCommand)
        {
            var token = _serviceJwtProvider.GenerateServiceToken(_identityOptions.ServiceName, new[] { Permissions.Profile.Create });

            var profileRequest = new InternalRequest<ProfileRequest<TId>>()
            {
                ClientName = _options.ClientName,
                Endpoint = _options.CreateEndpoint,
                Method = HttpMethod.Post,
                Body = profileCommand,
                Headers = new Dictionary<string, string>()
                {
                    { _identityOptions.HeaderName, $"{_identityOptions.Scheme} {token}" }
                }
            };

            var responseProfile = await _sendInternalRequest.SendAsync<ProfileRequest<TId>, bool>(profileRequest);

            if (!responseProfile.IsSuccess)
            {
                _logger.LogError("Profile sending failed. Client: {ClientName}, Endpoint: {Endpoint}, Login: {Login}, Reason: {ErrorReason}!",
                    profileRequest.ClientName,
                    profileRequest.Endpoint,
                    profileCommand.Login,
                    responseProfile.ErrorMessage
                );

                return Result.Failure(CommunicationError.SendError<ProfileSender>("Error creating user profile"));
            }

            return Result.Success();
        }

        public Task<Result> SendDeleteProfileRequestAsync<TId>(ProfileRequest<TId> profileCommand)
        {
            throw new NotImplementedException();
        }

        public Task<Result> SendUpdateProfileRequestAsync<TId>(ProfileRequest<TId> profileCommand)
        {
            throw new NotImplementedException();
        }
    }
}
