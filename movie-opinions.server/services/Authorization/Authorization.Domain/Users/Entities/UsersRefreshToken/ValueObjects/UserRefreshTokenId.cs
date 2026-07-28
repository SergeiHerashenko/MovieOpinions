using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects
{
    public sealed class UserRefreshTokenId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserRefreshTokenId(Guid value)
        {
            Value = value;
        }

        #region Creation
        internal static UserRefreshTokenId Create()
        {
            return new(Guid.CreateVersion7());
        }
        #endregion

        #region Restoration
        public static UserRefreshTokenId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserRefreshTokenId>(nameof(value));

            return new(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserRefreshTokenId data)
            => data.Value;
    }
}
