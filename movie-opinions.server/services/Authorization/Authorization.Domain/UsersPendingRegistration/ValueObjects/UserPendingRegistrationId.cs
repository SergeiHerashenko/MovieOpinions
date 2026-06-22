using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.UsersPendingRegistration.ValueObjects
{
    public sealed class UserPendingRegistrationId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserPendingRegistrationId(Guid value)
        {
            if(value == Guid.Empty)
                throw DomainDataInconsistencyException.EmptyOnRestore<UserPendingRegistrationId>(
                    nameof(value)
                );

            Value = value;
        }

        public static UserPendingRegistrationId CreateUnique()
        {
            return new(Guid.NewGuid());
        }

        public static UserPendingRegistrationId Restore(Guid value)
        {
            return new(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserPendingRegistrationId data)
            => data.Value;
    }
}
