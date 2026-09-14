using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

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
        /// Створює дію зміни пароля.
        ///
        /// (Creates a password change action.)
        /// </summary>
        public static UserAction From(Password newPassword)
            => ChangePasswordAction.Create(newPassword);

        /// <summary>
        /// Створює дію зміни логіна.
        ///
        /// (Creates a login change action.)
        /// </summary>
        public static UserAction From(Login login)
            => ChangeLoginAction.Create(login);

        /// <summary>
        /// Створює дію видалення користувача.
        ///
        /// (Creates a user deletion action.)
        /// </summary>
        public static UserAction From(DeletionReason reason)
            => DeleteUserAction.Create(reason);
    }
}
