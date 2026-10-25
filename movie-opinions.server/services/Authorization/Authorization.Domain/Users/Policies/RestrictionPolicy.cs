namespace Authorization.Domain.Users.Policies
{
    /// <summary>
    /// Визначає параметри системного блокування користувача
    /// через надмірну кількість невдалих спроб входу.
    ///
    /// (Defines the parameters of a user ban applied by the system
    /// after too many failed login attempts.)
    /// </summary>
    public static class RestrictionPolicy
    {
        public const string FailedLoginBanName = "System lock";

        public const int FailedLoginBanDurationMinutes = 10;

        public const string FailedLoginBanRestrictedBy = "System";

        public const string FailedLoginBanReason = "Too many failed login attempts";
    }
}
