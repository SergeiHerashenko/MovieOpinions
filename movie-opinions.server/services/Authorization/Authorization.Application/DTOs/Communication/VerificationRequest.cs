using Authorization.Application.Common.Enums;
using Authorization.Domain.Common.Models;

namespace Authorization.Application.DTOs.Communication
{
    public class VerificationRequest<TId>
    {
        public TId UserId { get; }

        public MessageActions MessageActions { get; }

        public string Code { get; }

        internal VerificationRequest(TId userId, MessageActions messageActions, string code)
        {
            UserId = userId;
            MessageActions = messageActions;
            Code = code;
        }
    }

    public static class VerificationRequest
    {
        public static VerificationRequest<TId> Create<TId>(
            AggregateRootId<TId> aggregateId,
            MessageActions messageActions,
            string code)
            => new(aggregateId.Value, messageActions, code);
    }
}
