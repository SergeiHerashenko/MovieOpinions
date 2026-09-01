namespace Authorization.MessageHandling.Localization
{
    internal static class ErrorMessageCatalog
    {
        public static IReadOnlyDictionary<
            string,
            IReadOnlyDictionary<string, string>> Messages
        { get; } =
            Merge(
                PasswordErrorMessages.Values,
                AcceptTermsRulesErrorMessages.Values,
                EmailErrorMessages.Values,
                PhoneErrorMessages.Values,
                GeneralErrorMessages.Values,
                DataErrorMessages.Values,
                IdentifierErrorMessages.Values
            );

        private static IReadOnlyDictionary<
            string, 
            IReadOnlyDictionary<string, string>> Merge(
                params IReadOnlyDictionary<
                    string, 
                    IReadOnlyDictionary<string, string>>[] catalogs)
        {
            return catalogs
                .SelectMany(catalog => catalog)
                .ToDictionary(
                    item => item.Key,
                    item => item.Value
                );
        }
    }
}
