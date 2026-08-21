using Authorization.Application.Abstractions.Communication;
using Authorization.Application.DTOs.Communication.Contacts.Requests;
using Authorization.Application.DTOs.Communication.Contacts.Responses;
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

        public Task<Result<GetActiveContactResponse>> GetActiveContactAsync<TId>(ActiveContactRequest<TId> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<GetActiveChannelsResponse>> GetActiveContactsAsync<TId>(ActiveChannelsRequest<TId> request, CancellationToken cancellationToken = default)
        {
            var userId = request.UserId?.ToString();

            if (string.IsNullOrWhiteSpace(userId))
                return Result<GetActiveChannelsResponse>.Failure(CommunicationError.SendError<ContactsSender>("User ID for contact request is missing!"));

            var token = _serviceJwtProvider.GenerateServiceToken(_identityOptions.ServiceName, new[] { Permissions.Contacts.Read });

            var escapedUserId = Uri.EscapeDataString(userId);

            var endpoint = _options.GetActiveEndpoint.Replace("{userId}", escapedUserId, StringComparison.Ordinal);

            var contactRequest = new InternalRequest
            {
                ClientName = _options.ClientName,
                Endpoint = endpoint,
                Method = HttpMethod.Get,
                Headers = new Dictionary<string, string>()
                {
                    { _identityOptions.HeaderName, $"{_identityOptions.Scheme} {token}" }
                }
            };

            var contactsResponse = await _sendInternalRequest.SendAsync<GetActiveChannelsResponse>(contactRequest, cancellationToken);

            if (!contactsResponse.IsSuccess)
            {
                _logger.LogError(
                    "Failed to get active contact channels. Client: {ClientName}, Endpoint: {Endpoint}, Reason: {ErrorReason}",
                    contactRequest.ClientName,
                    contactRequest.Endpoint,
                    contactsResponse.ErrorMessage
                );

                return Result<GetActiveChannelsResponse>.Failure(CommunicationError.SendError<ContactsSender>("Failed to receive active contact channels!"));
            }

            if(contactsResponse.Data is null)
            {
                _logger.LogError(
                    "Contact service returned an empty response body. Client: {ClientName}, Endpoint: {Endpoint}",
                    contactRequest.ClientName,
                    contactRequest.Endpoint
                );

                return Result<GetActiveChannelsResponse>.Failure(CommunicationError.SendError<ContactsSender>("Contact service returned an invalid response!"));
            }

            return Result<GetActiveChannelsResponse>.Success(contactsResponse.Data);
        }

        public async Task<Result> SendCreateContactRequestAsync<TId>(CreateContactRequest<TId> request, CancellationToken cancellationToken = default)
        {
            var token = _serviceJwtProvider.GenerateServiceToken(_identityOptions.ServiceName, new[] { Permissions.Contacts.Create });

            var contactRequest = new InternalRequest<CreateContactRequest<TId>>
            {
                ClientName = _options.ClientName,
                Endpoint = _options.CreateEndpoint,
                Method = HttpMethod.Post,
                Body = request,
                Headers = new Dictionary<string, string>()
                {
                    { _identityOptions.HeaderName, $"{_identityOptions.Scheme} {token}" }
                }
            };

            var contactsResponse = await _sendInternalRequest.SendAsync<CreateContactRequest<TId>, bool>(contactRequest);

            if (!contactsResponse.IsSuccess)
            {
                _logger.LogError("Contact creation failed. Client: {ClientName}, Endpoint: {Endpoint}, Reason: {ErrorReason}!",
                    contactRequest.ClientName,
                    contactRequest.Endpoint,
                    contactsResponse.ErrorMessage
                );

                return Result.Failure(CommunicationError.SendError<ContactsSender>("Failed to create user contact via integration service!"));
            }

            return Result.Success();
        }

        public Task<Result> SendDeleteContactRequestAsync<TId>(DeleteContactRequest<TId> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result> SendUpdateContactRequestAsync<TId>(UpdateContactRequest<TId> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
