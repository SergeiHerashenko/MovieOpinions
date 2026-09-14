namespace Authorization.Domain.Users.Entities.UsersPendingAction.Enums
{
    /// <summary>
    /// Визначає поточний стан життєвого циклу
    /// відкладеної дії користувача.
    ///
    /// (Defines the current lifecycle state of a pending user action.)
    /// </summary>
    public enum ActionStatus
    {
        /// <summary>
        /// Дія очікує підтвердження.
        ///
        /// (The action is awaiting confirmation.)
        /// </summary>
        Active = 0,

        /// <summary>
        /// Дію успішно підтверджено.
        ///
        /// (The action has been successfully confirmed.)
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// Дію скасовано до її підтвердження.
        ///
        /// (The action was cancelled before confirmation.)
        /// </summary>
        Cancelled = 2,

        /// <summary>
        /// Період підтвердження дії завершився.
        ///
        /// (The action confirmation period has expired.)
        /// </summary>
        Expired = 3,

        /// <summary>
        /// Дію не вдалося завершити, і вона перейшла
        /// в кінцевий стан помилки.
        ///
        /// (The action could not be completed and entered
        /// a terminal failure state.)
        /// </summary>
        Failed = 4,
    }
}
