using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.ValueObjects
{
    public sealed class UserId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserId(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserId>(nameof(value));

            Value = value;
        }

        public static UserId CreateUnique()
        {
            return new(Guid.CreateVersion7());
        }

        public static UserId Restore(Guid value)
        {
            return new(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserId data)
            => data.Value;
    }
}
