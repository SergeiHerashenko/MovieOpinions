using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersPendingChange.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Domain.Users.Entities.UsersPendingChange.Changes
{
    public abstract class UserChange : ValueObject
    {
        public abstract string Value { get; }

        public abstract UserChangeType UserChangeType { get; }

        public static UserChange From(Password newPassword) => new PasswordChange(newPassword);

        public static UserChange From(Login login) => new LoginChange(login);
    }
}
