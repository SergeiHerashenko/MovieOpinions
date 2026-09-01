namespace Authorization.ErrorHandling.CodesStatus
{
    internal static class CodesCatalog
    {
        public static Dictionary<string, int> Codes { get; } =
            Merge(
                DomainCodes.Values,
                ApplicationCodes.Values,
                InfrastructureCodes.Values
            );

        private static Dictionary<string, int> Merge(
            params Dictionary<string, int>[] catalogs)
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
