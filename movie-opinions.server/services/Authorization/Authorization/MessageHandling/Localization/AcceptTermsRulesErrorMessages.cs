using Authorization.Application.Common.Errors;

namespace Authorization.MessageHandling.Localization
{
    internal static class AcceptTermsRulesErrorMessages
    {
        public static IReadOnlyDictionary<
            string,
            IReadOnlyDictionary<string, string>> Values
        { get; } =
            new Dictionary<
                string, IReadOnlyDictionary<string, string>>
            {
                [ApplicationErrorCodes.RegistrationError.TermsMustBeAccepted] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Для продовження необхідно прийняти умови використання сервісу!",

                        [SupportedCultures.English] =
                            "To continue, you must accept the terms of service!"
                    },
            };
    }
}
