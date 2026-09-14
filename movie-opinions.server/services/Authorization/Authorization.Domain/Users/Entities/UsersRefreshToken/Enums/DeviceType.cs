namespace Authorization.Domain.Users.Entities.UsersRefreshToken.Enums
{
    /// <summary>
    /// Визначає загальну категорію пристрою,
    /// пов’язаного із сесією користувача.
    ///
    /// (Defines the general category of a device
    /// associated with a user session.)
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// Настільний комп’ютер або ноутбук.
        ///
        /// (A desktop or laptop computer.)
        /// </summary>
        Desktop = 0,

        /// <summary>
        /// Мобільний пристрій, наприклад смартфон.
        ///
        /// (A mobile device, such as a smartphone.)
        /// </summary>
        Mobile = 1,

        /// <summary>
        /// Планшетний пристрій.
        ///
        /// (A tablet device.)
        /// </summary>
        Tablet = 2,

        /// <summary>
        /// Тип пристрою не вдалося визначити.
        ///
        /// (The device type could not be determined.)
        /// </summary>
        Unknown = 3
    }
}
