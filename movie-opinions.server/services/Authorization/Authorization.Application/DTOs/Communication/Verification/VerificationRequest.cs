using Authorization.Application.DTOs.Communication.Verification.Enums;
using Authorization.Domain.Common.Models;

namespace Authorization.Application.DTOs.Communication.Verification
{
    public class VerificationRequest<TId>
    {
        public TId UserId { get; }

        public VerificationType VerificationType { get; }

        public string Code { get; }

        internal VerificationRequest(TId userId, VerificationType verificationType, string code)
        {
            UserId = userId;
            VerificationType = verificationType;
            Code = code;
        }
    }

    public static class VerificationRequest
    {
        public static VerificationRequest<TId> Create<TId>(
            AggregateRootId<TId> aggregateId,
            VerificationType verificationType,
            string code)
            => new(aggregateId.Value, verificationType, code);
    }
}
