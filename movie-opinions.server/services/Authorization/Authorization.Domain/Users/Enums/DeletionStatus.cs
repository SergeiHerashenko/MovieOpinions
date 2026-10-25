namespace Authorization.Domain.Users.Enums
{
    /// <summary>
    /// Визначає стан видалення та відновлення користувача.
    ///
    /// (Defines the state of user deletion and restoration.)
    /// </summary>
    public enum DeletionStatus
    {
        /// <summary>
        /// Користувача позначено як видаленого.
        ///
        /// (The user is marked as deleted.)
        /// </summary>
        Deleted = 0,

        /// <summary>
        /// Користувача відновлено після видалення.
        ///
        /// (The user has been restored after deletion.)
        /// </summary>
        Restored = 1,

        /// <summary>
        /// Видалення користувача позначено як остаточне.
        ///
        /// (The user deletion is marked as permanent.)
        /// </summary>
        PermanentlyDeleted = 2
    }
}
