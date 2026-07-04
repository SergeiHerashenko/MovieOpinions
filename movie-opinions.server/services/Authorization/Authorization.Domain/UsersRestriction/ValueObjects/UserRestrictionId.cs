using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.UsersRestriction.ValueObjects
{
    public sealed class UserRestrictionId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserRestrictionId(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserRestrictionId>(nameof(value));

            Value = value;
        }

        public static UserRestrictionId CreateUnique()
        {
            return new(Guid.CreateVersion7());
        }

        public static UserRestrictionId Restore(Guid value)
        {
            return new(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserRestrictionId data)
            => data.Value;
    }
}
