using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.UsersDeletion.ValueObjects
{
    public sealed class UserDeletionId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserDeletionId(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"Cannot restore entity because identifier is invalid (empty GUID). Entity {nameof(UserDeletionId)}",
                    new Dictionary<string, object>
                    {
                        ["entity"] = nameof(UserDeletionId),
                        ["operation"] = "restore",
                        ["value"] = value
                    }
                );

            Value = value;
        }

        public static UserDeletionId CreateUnique()
        {
            return new(Guid.NewGuid());
        }

        public static UserDeletionId Restore(Guid value)
        {
            return new(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserDeletionId data)
            => data.Value;
    }
}
