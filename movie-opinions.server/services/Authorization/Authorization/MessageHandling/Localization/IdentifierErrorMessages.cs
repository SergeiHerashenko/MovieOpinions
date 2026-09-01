using Authorization.Domain.Common.Errors;

namespace Authorization.MessageHandling.Localization
{
    internal class IdentifierErrorMessages
    {
        public static IReadOnlyDictionary<
            string,
            IReadOnlyDictionary<string, string>> Values
        { get; } =
            new Dictionary<
                string,
                IReadOnlyDictionary<string, string>>
            {
                [DomainErrorCodes.Identifier.Empty] = 
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Ідентифікатор є обов'язковим!",

                        [SupportedCultures.English] =
                            "Identifier is required!"
                    },

                [DomainErrorCodes.Identifier.IdentifierMismatch] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Ідентифікатори не збігаються!",

                        [SupportedCultures.English] =
                            "Identifiers do not match!"
                    }
            };
    }
}
