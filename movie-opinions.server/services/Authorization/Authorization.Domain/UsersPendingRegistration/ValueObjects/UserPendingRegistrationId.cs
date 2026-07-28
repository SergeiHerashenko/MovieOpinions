using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.UsersPendingRegistration.ValueObjects
{
    public sealed class UserPendingRegistrationId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserPendingRegistrationId(Guid value)
        {
            Value = value;
        }

        #region Creation
        public static UserPendingRegistrationId Create()
        {
            return new(Guid.CreateVersion7());
        }
        #endregion

        #region Restoration
        public static UserPendingRegistrationId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserPendingRegistrationId>(nameof(value));

            return new(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserPendingRegistrationId data)
            => data.Value;
    }
}
