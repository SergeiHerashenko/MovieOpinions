namespace Authorization.Domain.Users.Enums
{
    /// <summary>
    /// Визначає роль користувача в доменній моделі.
    ///
    /// (Defines the user's role in the domain model.)
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// Роль гостя.
        ///
        /// (Guest role.)
        /// </summary>
        Guest = 0,

        /// <summary>
        /// Роль звичайного користувача.
        ///
        /// (Regular user role.)
        /// </summary>
        User = 1,

        /// <summary>
        /// Роль модератора.
        ///
        /// (Moderator role.)
        /// </summary>
        Moderator = 2,

        /// <summary>
        /// Роль адміністратора.
        ///
        /// (Administrator role.)
        /// </summary>
        Admin = 3
    }
}
