using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects
{
    public sealed class UserPendingActionId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserPendingActionId(Guid value)
        {
            Value = value;
        }

        #region Creation
        internal static UserPendingActionId Create()
        {
            return new(Guid.CreateVersion7());
        }
        #endregion

        #region Restoration
        public static UserPendingActionId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserPendingActionId>(nameof(value));

            return new(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserPendingActionId data)
            => data.Value;
    }
}
