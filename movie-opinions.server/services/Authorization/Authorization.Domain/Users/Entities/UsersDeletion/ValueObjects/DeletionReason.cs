using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;

namespace Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects
{
    public sealed class DeletionReason : ValueObject
    {
        private static readonly int MAX_REASON_LENGTH = 600;
        private static readonly string DEFAULT_REASON = "Reason is empty";

        public string Value { get; }

        private DeletionReason(string value)
        {
            Value = value;
        }

        #region Creation
        internal static Result<DeletionReason> Create(string? rawReason)
        {
            if (string.IsNullOrWhiteSpace(rawReason))
                rawReason = DEFAULT_REASON;

            var trimRawReason = rawReason.Trim();

            if (trimRawReason.Length > MAX_REASON_LENGTH)
                return Result<DeletionReason>.Failure(DeletionErrors.TooLongReason<DeletionReason>());

            return Result<DeletionReason>.Success(new DeletionReason(trimRawReason));
        }
        #endregion

        #region Restoration
        public static DeletionReason Restore(string value)
        {
            var trimRawReason = value.Trim();

            if (string.IsNullOrWhiteSpace(trimRawReason))
                throw DomainDataInconsistencyException.Empty<DeletionReason>(nameof(trimRawReason));

            if (trimRawReason.Length > MAX_REASON_LENGTH)
                throw DomainDataInconsistencyException.ValueOutOfRange<DeletionReason>(nameof(trimRawReason));

            return new(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
