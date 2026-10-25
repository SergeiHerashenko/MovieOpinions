namespace Authorization.Domain.Users.Entities.UsersPendingAction.Enums
{
    /// <summary>
    /// Визначає тип відкладеної дії, яку користувач
    /// повинен підтвердити.
    ///
    /// (Defines the type of pending action
    /// that must be confirmed by the user.)
    /// </summary>
    public enum UserActionType
    {
        /// <summary>
        /// Зміна пароля користувача.
        ///
        /// (User password change.)
        /// </summary>
        ChangePassword = 0,

        /// <summary>
        /// Зміна логіна користувача.
        ///
        /// (User login change.)
        /// </summary>
        ChangeLogin = 1,

        /// <summary>
        /// Видалення користувача.
        ///
        /// (User deletion.)
        /// </summary>
        DeleteUser = 2
    }
}
