namespace Authorization.Domain.Users.Enums
{
    /// <summary>
    /// Визначає вид обмеження, застосованого до користувача.
    ///
    /// (Defines the kind of restriction applied to a user.)
    /// </summary>
    public enum RestrictionType
    {
        /// <summary>
        /// Блокування доступу користувача.
        ///
        /// (Blocks user access.)
        /// </summary>
        Ban = 0,

        /// <summary>
        /// Обмеження можливості залишати коментарі.
        ///
        /// (Restricts the ability to post comments.)
        /// </summary>
        CommentMute = 1,

        /// <summary>
        /// Обмеження можливості надсилати повідомлення в чаті.
        ///
        /// (Restricts the ability to send chat messages.)
        /// </summary>
        ChatMute = 2,

        /// <summary>
        /// Одночасне обмеження коментування та надсилання повідомлень у чаті.
        ///
        /// (Restricts both posting comments and sending chat messages.)
        /// </summary>
        AllMute = 3
    }
}
