using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.Entities.UsersPendingChange.ValueObjects
{
    public sealed class UserPendingChangeId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserPendingChangeId(Guid value)
        {
            Value = value;
        }

        #region Creation
        internal static UserPendingChangeId Create()
        {
            return new(Guid.CreateVersion7());
        }
        #endregion

        #region Restoration
        public static UserPendingChangeId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserPendingChangeId>(nameof(value));

            return new(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserPendingChangeId data)
            => data.Value;
    }
}
