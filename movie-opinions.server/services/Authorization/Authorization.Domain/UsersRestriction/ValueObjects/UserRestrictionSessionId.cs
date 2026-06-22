using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.UsersRestriction.ValueObjects
{
    public sealed class UserRestrictionSessionId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserRestrictionSessionId(Guid value)
        {
            if(value == Guid.Empty)
                throw DomainDataInconsistencyException.EmptyOnRestore<UserRestrictionSessionId>(
                    nameof(value)
                );

            Value = value;
        }

        public static UserRestrictionSessionId CreateUnique()
        {
            return new(Guid.NewGuid());
        }

        public static UserRestrictionSessionId Restore(Guid value)
        {
            return new(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserRestrictionSessionId data)
            => data.Value;
    }
}
