using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects
{
    public sealed class UserDeletionId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserDeletionId(Guid value)
        {
            Value = value;
        }

        #region Creation
        internal static UserDeletionId Create()
        {
            return new(Guid.CreateVersion7());
        }
        #endregion

        #region Restoration
        public static UserDeletionId Restore(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserDeletionId>(nameof(value));

            return new(value);
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserDeletionId data)
            => data.Value;
    }
}
