using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.UsersDeletion.ValueObjects
{
    public sealed class UserDeletionId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserDeletionId(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserDeletionId>(nameof(value));

            Value = value;
        }

        public static UserDeletionId CreateUnique()
        {
            return new(Guid.CreateVersion7());
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
