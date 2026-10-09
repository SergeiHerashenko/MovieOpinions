namespace Authorization.Domain.Users.Entities.UsersRestriction.Enums
{
    /// <summary>
    /// Визначає поточний стан обмеження користувача.
    ///
    /// (Defines the current state of a user restriction.)
    /// </summary>
    public enum RestrictionStatus
    {
        /// <summary>
        /// Обмеження активне та продовжує застосовуватися до користувача.
        ///
        /// (The restriction is active and continues to apply to the user.)
        /// </summary>
        Active = 0,

        /// <summary>
        /// Користувач повністю відбув встановлений строк обмеження.
        ///
        /// (The user has fully completed the configured restriction period.)
        /// </summary>
        Completed = 1,

        /// <summary>
        /// Обмеження було достроково зняте до природного завершення строку.
        ///
        /// (The restriction was revoked before its natural completion.)
        /// </summary>
        Revoked = 2,
    }
}
