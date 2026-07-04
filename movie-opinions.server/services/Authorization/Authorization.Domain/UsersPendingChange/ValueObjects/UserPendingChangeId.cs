using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.UsersPendingChange.ValueObjects
{
    public sealed class UserPendingChangeId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserPendingChangeId(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserPendingChangeId>(nameof(value));

            Value = value;
        }

        public static UserPendingChangeId CreateUnique()
        {
            return new(Guid.CreateVersion7());
        }

        public static UserPendingChangeId Restore(Guid value)
        {
            return new(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserPendingChangeId data)
            => data.Value;
    }
}
