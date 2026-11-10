namespace Authorization.Domain.Users.Entities.UsersRestrictionSession.Enums
{
    /// <summary>
    /// Описує вплив вилучення обмеження на стан сесії обмежень користувача.
    ///
    /// (Describes the effect of removing a restriction on the state of a user restriction session.)
    /// </summary>
    internal enum RestrictionSessionRemovalEffect
    {
        /// <summary>
        /// Сесія залишається активною.
        ///
        /// (The session remains active.)
        /// </summary>
        RemainsActive,

        /// <summary>
        /// Після вилучення у сесії не залишається обмежень.
        ///
        /// (The session contains no restrictions after removal.)
        /// </summary>
        BecomesEmpty,

        /// <summary>
        /// Після вилучення обмеження сесія стає простроченою.
        ///
        /// (The session becomes expired after the restriction is removed.)
        /// </summary>
        BecomesExpired,

        /// <summary>
        /// Час завершення строку дії сесії вже настав до вилучення обмежень.
        ///
        /// (The session expiration time has been reached before any restrictions are removed.)
        /// </summary>
        AlreadyExpired
    }
}
