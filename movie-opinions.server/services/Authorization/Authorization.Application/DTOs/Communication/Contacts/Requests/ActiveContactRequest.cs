using Authorization.Domain.Common.Models;

namespace Authorization.Application.DTOs.Communication.Contacts.Requests
{
    public sealed class ActiveContactRequest<TId>
    {
        public TId UserId { get; }

        public Guid ContactId { get; }

        internal ActiveContactRequest(TId userId, Guid contactId)
        {
            UserId = userId;
            ContactId = contactId;
        }
    }

    public static class ActiveContactRequest
    {
        public static ActiveContactRequest<TId> Create<TId>(AggregateRootId<TId> userId, Guid contactId)
        {
            return new ActiveContactRequest<TId>(userId.Value, contactId);
        }
    }
}
