using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects
{
    public sealed class UserRestrictionId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserRestrictionId(Guid value)
        {
            Value = value;
        }

        #region Creation
        internal static UserRestrictionId Create()
        {
            return new(Guid.CreateVersion7());
        }
        #endregion

        #region Restoration
        public static UserRestrictionId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserRestrictionId>(nameof(value));

            return new(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserRestrictionId data)
            => data.Value;
    }
}
