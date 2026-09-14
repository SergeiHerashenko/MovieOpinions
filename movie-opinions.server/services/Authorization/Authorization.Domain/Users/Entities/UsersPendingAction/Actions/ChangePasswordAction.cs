using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.Actions
{
    /// <summary>
    /// Представляє дані відкладеної дії зі зміни пароля користувача.
    ///
    /// (Represents the data of a pending user password-change action.)
    /// </summary>
    public sealed class ChangePasswordAction : UserAction
    {
        /// <summary>
        /// Новий пароль, який буде встановлено після підтвердження дії.
        ///
        /// (New password that will be applied after action confirmation.)
        /// </summary>
        public Password NewPassword { get; }

        public override UserActionType ActionType => UserActionType.ChangePassword;

        private ChangePasswordAction(Password newPassword)
        {
            NewPassword = newPassword;
        }

        public override string Value => NewPassword.Value;

        #region Creation
        /// <summary>
        /// Створює дію зміни пароля з валідного доменного пароля.
        ///
        /// (Creates a password-change action from a valid domain password.)
        /// </summary>
        internal static ChangePasswordAction Create(Password passwordHash)
        {
            DomainGuard.AgainstNull<ChangePasswordAction>(
                OperationType.Create,
                (passwordHash, nameof(passwordHash))
            );

            return new ChangePasswordAction(passwordHash);
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює дію зміни пароля зі збереженого хешу.
        ///
        /// (Restores a password-change action from a persisted password hash.)
        /// </summary>
        /// <param name="passwordHash">Збережений хеш нового пароля.</param>
        public static ChangePasswordAction Restore(PasswordHash hashPassword)
        {
            DomainGuard.AgainstNull<ChangePasswordAction>(
                OperationType.Restore,
                (hashPassword, nameof(hashPassword))
            );

            return new ChangePasswordAction(Password.Restore(hashPassword));
        }
        #endregion

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return NewPassword;
        }
    }
}
