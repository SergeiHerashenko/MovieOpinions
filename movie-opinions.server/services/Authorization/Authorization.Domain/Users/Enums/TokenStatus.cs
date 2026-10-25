namespace Authorization.Domain.Users.Enums
{
    /// <summary>
    /// Визначає стан токена в його життєвому циклі.
    ///
    /// (Defines the lifecycle state of a token.)
    /// </summary>
    public enum TokenStatus
    {
        /// <summary>
        /// Токен позначено як активний.
        ///
        /// (The token is marked as active.)
        /// </summary>
        Active = 0,

        /// <summary>
        /// Строк дії токена завершився.
        ///
        /// (The token's lifetime has expired.)
        /// </summary>
        Expired = 1,

        /// <summary>
        /// Токен позначено як використаний.
        ///
        /// (The token is marked as consumed.)
        /// </summary>
        Consumed = 2,

        /// <summary>
        /// Токен відкликано.
        ///
        /// (The token has been revoked.)
        /// </summary>
        Revoked = 3
    }
}
