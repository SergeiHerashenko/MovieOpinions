using Authorization.Domain.Common.Models;

namespace Authorization.Application.DTOs.Communication.Contacts.Requests
{
    public sealed class ActiveChannelsRequest<TId>
    {
        public TId UserId { get; }

        internal ActiveChannelsRequest(TId userId)
        {
            UserId = userId;
        }
    }

    public static class ActiveChannelsRequest
    {
        public static ActiveChannelsRequest<TId> Create<TId>(AggregateRootId<TId> userId)
        {
            return new ActiveChannelsRequest<TId>(userId.Value);
        }
    }
}
