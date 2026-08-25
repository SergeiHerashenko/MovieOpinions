using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Abstractions.Services.UserActiveContacts;
using Authorization.Application.Common.Errors.Users;
using Authorization.Application.DTOs.Communication.Contacts.Requests;
using Authorization.Application.Features.Services.UserActiveContacts.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Features.Services.UserActiveContacts
{
    public sealed class ActiveContactsProvider : IActiveContactsProvider
    {
        private readonly IContactsSender _contactsSender;
        private readonly ILogger<ActiveContactsProvider> _logger;

        public ActiveContactsProvider(
            IContactsSender contactsSender,
            ILogger<ActiveContactsProvider> logger)
        {
            _contactsSender = contactsSender;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyCollection<ActiveContactChannel>>> GetActiveContactsAsync(
            UserId userId, 
            CancellationToken cancellationToken = default)
        {
            var request = ActiveChannelsRequest.Create(userId);

            var contactsResult = await _contactsSender.GetActiveContactsAsync(
                request, 
                cancellationToken
            );

            if (contactsResult.IsFailure)
            {
                return Result<IReadOnlyCollection<ActiveContactChannel>>
                    .Failure(contactsResult.Errors);
            }
                

            if (contactsResult.Value.Channels.Count == 0)
            {
                _logger.LogCritical(
                    "User {UserId} has no active verified contact channels. The user-contact invariant is violated!",
                    userId.Value
                );

                return Result<IReadOnlyCollection<ActiveContactChannel>>.
                    Failure(ContactErrors.ContactInvariantViolated<ActiveContactsProvider>(userId.Value.ToString()));
            }
            
            var channels = contactsResult.Value.Channels
                .Select(contact => new ActiveContactChannel(
                    contact.ContactId,
                    contact.Channel,
                    contact.MaskedValue))
                .ToArray();

            return Result<IReadOnlyCollection<ActiveContactChannel>>.Success(channels);
        }
    }
}
