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
    public class ContactsSender : IContactsSender
    {
        private readonly ContactsServiceOptions _options;
        private readonly ServiceIdentityOptions _identityOptions;
        private readonly ILogger<ContactsSender> _logger;
        private readonly ISendInternalRequest _sendInternalRequest;
        private readonly IServiceJwtProvider _serviceJwtProvider;

        public ContactsSender(
            IOptions<ContactsServiceOptions> options,
            IOptions<ServiceIdentityOptions> identityOptions,
            ILogger<ContactsSender> logger,
            ISendInternalRequest sendInternalRequest,
            IServiceJwtProvider serviceJwtProvider)
        {
            _options = options.Value;
            _identityOptions = identityOptions.Value;
            _logger = logger;
            _sendInternalRequest = sendInternalRequest;
            _serviceJwtProvider = serviceJwtProvider;
        }

        public async Task<Result> SendCreateContactRequestAsync<TId>(ContactsRequest<TId> contactsRequest)
        {
            var token = _serviceJwtProvider.GenerateServiceToken(_identityOptions.ServiceName, new[] { Permissions.Contacts.Create });

            var contactRequest = new InternalRequest<ContactsRequest<TId>>
            {
                ClientName = _options.ClientName,
                Endpoint = _options.CreateEndpoint,
                Method = HttpMethod.Post,
                Body = contactsRequest,
                Headers = new Dictionary<string, string>()
                {
                    { _identityOptions.HeaderName, $"{_identityOptions.Scheme} {token}" }
                }
            };

            var responseContacts = await _sendInternalRequest.SendAsync<ContactsRequest<TId>, bool>(contactRequest);

            if (!responseContacts.IsSuccess)
            {
                _logger.LogError("Contact creation failed. Client: {ClientName}, Endpoint: {Endpoint}, Reason: {ErrorReason}!",
                    contactRequest.ClientName,
                    contactRequest.Endpoint,
                    responseContacts.ErrorMessage
                );

                return Result.Failure(CommunicationError.SendError<ContactsSender>("Failed to create user contact via integration service!"));
            }

            return Result.Success();
        }

        public Task<Result> SendDeleteContactRequestAsync<TId>(ContactsRequest<TId> contactsRequest)
        {
            throw new NotImplementedException();
        }

        public Task<Result> SendUpdateContactRequestAsync<TId>(ContactsRequest<TId> contactsRequest)
        {
            throw new NotImplementedException();
        }
    }
}
