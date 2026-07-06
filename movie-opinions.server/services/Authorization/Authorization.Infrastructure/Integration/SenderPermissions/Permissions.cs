namespace Authorization.Infrastructure.Integration.SenderPermissions
{
    public static class Permissions
    {
        public static class Contacts
        {
            public const string Create = "contacts:create";

            public const string Update = "contacts:update";

            public const string Delete = "contacts:delete";
        }

        public static class Profile
        {
            public const string Create = "profile:create";

            public const string Update = "profile:update";

            public const string Delete = "profile:delete";
        }

        public static class Notification
        {
            public const string Create = "notification:create";
        }

        public static class Verification
        {
            public const string Audit = "vefification:audit";
        }
    }
}
