using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;

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

        /// <summary>
        /// Текстове значення хешу нового пароля.
        ///
        /// (The textual hash value of the new password.)
        /// </summary>
        public override string Value => NewPassword.Value;

        #region Creation
        /// <summary>
        /// Створює дані відкладеної дії зі зміни пароля
        /// з попередньо створеного доменного пароля.
        ///
        /// (Creates pending password-change action data
        /// from a previously created domain password.)
        /// </summary>
        /// <param name="passwordHash">
        /// Новий доменний пароль, що містить його хеш.
        ///
        /// (The new domain password containing its hash.)
        /// </param>
        /// <returns>
        /// Створена дія зміни пароля.
        ///
        /// (The created password-change action.)
        /// </returns>
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
        /// Відновлює дані відкладеної дії зі зміни пароля
        /// з попередньо відновленого хешу.
        ///
        /// (Restores pending password-change action data
        /// from a previously restored password hash.)
        /// </summary>
        /// <param name="hashPassword">
        /// Відновлений хеш нового пароля.
        ///
        /// (The restored hash of the new password.)
        /// </param>
        /// <returns>
        /// Відновлена дія зміни пароля.
        ///
        /// (The restored password-change action.)
        /// </returns>
        public static ChangePasswordAction Restore(PasswordHash hashPassword)
        {
            DomainGuard.AgainstNull<ChangePasswordAction>(
                OperationType.Restore,
                (hashPassword, nameof(hashPassword))
            );

            return new ChangePasswordAction(Password.Restore(hashPassword));
        }
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return NewPassword;
        }
    }
}
