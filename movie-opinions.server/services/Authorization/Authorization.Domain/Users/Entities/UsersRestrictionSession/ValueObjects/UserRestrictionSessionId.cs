using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects
{
    public sealed class UserRestrictionSessionId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserRestrictionSessionId(Guid value)
        {
            Value = value;
        }

        #region Creation
        internal static UserRestrictionSessionId Create()
        {
            return new(Guid.CreateVersion7());
        }
        #endregion

        #region Restoration
        public static UserRestrictionSessionId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserRestrictionSessionId>(nameof(value));

            return new(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserRestrictionSessionId data)
            => data.Value;
    }
}
