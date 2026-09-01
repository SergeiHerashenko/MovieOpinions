namespace Authorization.MessageHandling.Localization
{
    internal static class SupportedCultures
    {
        public const string Ukrainian = "uk";

        public const string English = "en";

        public const string Default = English;

        public static IReadOnlySet<string> All { get; } =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Ukrainian,
                English
            };
    }
}
