using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;

namespace Authorization.Domain.Users.Entities.UsersPendingAction.Actions
{
    /// <summary>
    /// Базовий об’єкт-значення, який представляє дані
    /// відкладеної дії користувача.
    /// Конкретний тип визначає вид дії та пов’язані з нею дані.
    ///
    /// (Base value object representing the data of a pending user action.
    /// The concrete type determines the action kind and its associated data.)
    /// </summary>
    public abstract class UserAction : ValueObject
    {
        /// <summary>
        /// Тип відкладеної дії, який відповідає конкретній реалізації.
        ///
        /// (Pending-action type corresponding to the concrete implementation.)
        /// </summary>
        public abstract UserActionType ActionType { get; }

        /// <summary>
        /// Скалярне представлення даних відкладеної дії.
        /// Семантика значення визначається конкретним типом дії.
        ///
        /// (Scalar representation of the pending-action data.
        /// The concrete action type defines the value semantics.)
        /// </summary>
        public abstract string Value { get; }

        /// <summary>
        /// Створює дані відкладеної дії зі зміни пароля користувача.
        ///
        /// (Creates pending user password-change action data.)
        /// </summary>
        /// <param name="newPassword">
        /// Новий доменний пароль, що містить його хеш.
        ///
        /// (The new domain password containing its hash.)
        /// </param>
        /// <returns>
        /// Дія зміни пароля.
        ///
        /// (A password-change action.)
        /// </returns>
        public static UserAction From(Password newPassword)
            => ChangePasswordAction.Create(newPassword);

        /// <summary>
        /// Створює дані відкладеної дії зі зміни логіна користувача.
        ///
        /// (Creates pending user login-change action data.)
        /// </summary>
        /// <param name="login">
        /// Новий доменний логін користувача.
        ///
        /// (The new domain user login.)
        /// </param>
        /// <returns>
        /// Дія зміни логіна.
        ///
        /// (A login-change action.)
        /// </returns>
        public static UserAction From(Login login)
            => ChangeLoginAction.Create(login);

        /// <summary>
        /// Створює дані відкладеної дії з видалення користувача.
        ///
        /// (Creates pending user-deletion action data.)
        /// </summary>
        /// <param name="reason">
        /// Доменне представлення причини видалення.
        ///
        /// (The domain representation of the deletion reason.)
        /// </param>
        /// <returns>
        /// Дія видалення користувача.
        ///
        /// (A user-deletion action.)
        /// </returns>
        public static UserAction From(DeletionReason reason)
            => DeleteUserAction.Create(reason);
    }
}
