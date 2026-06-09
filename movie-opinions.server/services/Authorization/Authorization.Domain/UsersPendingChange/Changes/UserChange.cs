using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingChange.Enums;

namespace Authorization.Domain.UsersPendingChange.Changes
{
    public abstract class UserChange : ValueObject
    {
        public abstract string Value { get; }

        public abstract UserChangeType Type { get; }

        public static UserChange From(Password newPassword) => new PasswordChange(newPassword);

        public static UserChange From(Login login) => new LoginChange(login);
    }
}
