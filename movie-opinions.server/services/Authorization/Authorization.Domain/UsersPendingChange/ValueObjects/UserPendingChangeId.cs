using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.UsersPendingChange.ValueObjects
{
    public sealed class UserPendingChangeId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserPendingChangeId(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"Cannot restore entity because identifier is invalid (empty GUID). Entity {nameof(UserPendingChangeId)}",
                    new Dictionary<string, object>
                    {
                        ["entity"] = nameof(UserPendingChangeId),
                        ["operation"] = "restore",
                        ["value"] = value
                    }
                );

            Value = value;
        }

        public static UserPendingChangeId CreateUnique()
        {
            return new(Guid.NewGuid());
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
