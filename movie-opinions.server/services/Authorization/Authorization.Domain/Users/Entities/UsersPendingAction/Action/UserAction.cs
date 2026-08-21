using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.Action
{
    public abstract class UserAction : ValueObject
    {
        public abstract UserActionType ActionType { get; }

        public static UserAction From(Password newPassword) => new PasswordChangeAction(newPassword);

        public static UserAction From(Login login) => new LoginChangeAction(login);

        public static UserAction From(DeletionReason reason) => new DeleteAccountAction(reason);
    }
}
