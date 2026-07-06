using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.DTOs.Communication
{
    public class ProfileRequest<TId>
    {
        public TId UserId { get; }

        public string Login { get; }

        public Role Role { get; }

        internal ProfileRequest(
            TId userId,
            string login,
            Role role)
        {
            UserId = userId;
            Login = login;
            Role = role;
        }
    }

    public static class ProfileRequest
    {
        public static ProfileRequest<TId> Create<TId>(AggregateRootId<TId> aggregateId, Login login, Role role)
            => new(aggregateId.Value, login.Value, role);
    }
}
