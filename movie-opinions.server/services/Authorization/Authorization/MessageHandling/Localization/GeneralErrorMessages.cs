using Authorization.Domain.Common.Errors;

namespace Authorization.MessageHandling.Localization
{
    internal class GeneralErrorMessages
    {
        public static IReadOnlyDictionary<
            string,
            IReadOnlyDictionary<string, string>> Values
        { get; } =
            new Dictionary<
                string,
                IReadOnlyDictionary<string, string>>
            {
                [DomainErrorCodes.General.InvalidState] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Операція недоступна у поточному стані об'єкта!",

                        [SupportedCultures.English] =
                            "The action cannot be performed in the current state!"
                    },

                [DomainErrorCodes.General.InvalidOperation] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Цю дію неможливо виконати!",

                        [SupportedCultures.English] =
                            "This operation cannot be performed!"
                    },

                [DomainErrorCodes.General.UnsupportedType] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Цей тип даних або файлу не підтримується!",

                        [SupportedCultures.English] =
                            "This data or file type is not supported!"
                    },

                [DomainErrorCodes.General.NoUpdateNeeded] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Оновлення не потрібні!",

                        [SupportedCultures.English] =
                            "No updates needed!"
                    },

                [DomainErrorCodes.General.Expired] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Термін дії цієї дії, коду або сесії вичерпано!",

                        [SupportedCultures.English] =
                            "This action, code, or session has expired!"
                    },

                [DomainErrorCodes.General.ActionCancelled] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Операцію скасовано!",

                        [SupportedCultures.English] =
                            "The operation was cancelled!"
                    },
            };
    }
}
