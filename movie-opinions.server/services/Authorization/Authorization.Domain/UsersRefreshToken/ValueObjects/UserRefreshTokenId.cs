using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Models;

namespace Authorization.Domain.UsersRefreshToken.ValueObjects
{
    public sealed class UserRefreshTokenId : AggregateRootId<Guid>
    {
        public override Guid Value { get; protected set; }

        private UserRefreshTokenId(Guid value)
        {
            if (value == Guid.Empty)
                throw DomainDataInconsistencyException.Empty<UserRefreshTokenId>(nameof(value));

            Value = value;
        }

        public static UserRefreshTokenId CreateUnique()
        {
            return new(Guid.CreateVersion7());
        }

        public static UserRefreshTokenId Restore(Guid value)
        {
            return new(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator Guid(UserRefreshTokenId data)
            => data.Value;
    }
}
