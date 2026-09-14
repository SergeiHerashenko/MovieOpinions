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
        /// Підтвердження зміни пароля.
        ///
        /// (Password change confirmation.)
        /// </summary>
        ChangePassword = 0,

        /// <summary>
        /// Підтвердження зміни логіна.
        ///
        /// (Login change confirmation.)
        /// </summary>
        ChangeLogin = 1,

        /// <summary>
        /// Підтвердження видалення користувача.
        ///
        /// (User deletion confirmation.)
        /// </summary>
        DeleteUser = 2
    }
}
