namespace Authorization.Domain.Users.Entities.UsersRestriction.Enums
{
    /// <summary>
    /// Визначає збережений стан життєвого циклу
    /// обмеження користувача.
    ///
    /// (Defines the stored lifecycle state
    /// of a user restriction.)
    /// </summary>
    public enum RestrictionStatus
    {
        /// <summary>
        /// Обмеження ще не переведене у завершений або відкликаний стан.
        /// Фактична дія обмеження у певний момент визначається
        /// часовими правилами пов’язаної сесії.
        ///
        /// (The restriction has not transitioned to a completed
        /// or revoked state. Its effective application at a given time
        /// is determined by the timing rules of the associated session.)
        /// </summary>
        Active = 0,

        /// <summary>
        /// Обмеження завершене відповідно до часових правил
        /// пов’язаної сесії, зокрема після перерахунку її тривалості.
        ///
        /// (The restriction has completed under the timing rules
        /// of its associated session, including after duration recalculation.)
        /// </summary>
        Completed = 1,

        /// <summary>
        /// Обмеження було достроково відкликане.
        ///
        /// (The restriction was revoked early.)
        /// </summary>
        Revoked = 2,
    }
}
