namespace Authorization.Domain.Common.Enums
{
    /// <summary>
    /// Визначає тип логіна користувача.
    ///
    /// (Defines the user login type.)
    /// </summary>
    public enum LoginType
    {
        /// <summary>
        /// Логін на основі телефонного номера.
        ///
        /// (Login based on a telephone number.)
        /// </summary>
        Phone = 0,

        /// <summary>
        /// Логін на основі email-адреси.
        ///
        /// (Login based on an email address.)
        /// </summary>
        Email = 1
    }
}
