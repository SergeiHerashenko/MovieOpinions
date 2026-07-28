namespace Authorization.Domain.Users.Policies
{
    public static class RestrictionPolicy
    {
        public const string FailedLoginBanName = "System lock";

        public const int FailedLoginBanDurationMinutes = 10;

        public const string FailedLoginBanRestrictedBy = "System";

        public const string FailedLoginBanReason = "Too many failed login attempts";
    }
}
