using Authorization.Domain.Common.Errors;

namespace Authorization.MessageHandling.Localization
{
    internal static class EmailErrorMessages
    {
        public static IReadOnlyDictionary<
            string,
            IReadOnlyDictionary<string, string>> Values
        { get; } =
            new Dictionary<
                string,
                IReadOnlyDictionary<string, string>>
            {
                [DomainErrorCodes.Email.EmptyEmail] = 
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Вкажіть електронну пошту!",

                        [SupportedCultures.English] =
                            "Email address is required!"
                    },

                [DomainErrorCodes.Email.InvalidFormatEmail] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Некоректний формат електронної пошти!",

                        [SupportedCultures.English] =
                            "Invalid email address format!"
                    },

                [DomainErrorCodes.Email.EmptyEmailDomain] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Вкажіть домен пошти (частину після @)!",

                        [SupportedCultures.English] =
                            "Email domain (after @) is required!"
                    },

                [DomainErrorCodes.Email.NotAllowedEmailDomain] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Цей домен електронної пошти не підтримується!",

                        [SupportedCultures.English] =
                            "This email domain is not allowed!"
                    },

                [DomainErrorCodes.Email.InvalidFormatEmailDomainPart] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Некоректний формат домену пошти!",

                        [SupportedCultures.English] =
                            "Invalid format of the email domain!"
                    },

                [DomainErrorCodes.Email.TooLongEmailDomainPart] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Домен пошти занадто довгий!",

                        [SupportedCultures.English] =
                            "Email domain is too long!"
                    },

                [DomainErrorCodes.Email.TooShortEmailDomainPart] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Домен пошти занадто короткий!",

                        [SupportedCultures.English] =
                            "Email domain is too short!"
                    },

                [DomainErrorCodes.Email.EmptyEmailLocalPart] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Вкажіть ім'я користувача пошти (частину перед @)!",

                        [SupportedCultures.English] =
                            "Email local part (before @) is required!"
                    },

                [DomainErrorCodes.Email.InvalidFormatEmailLocalPart] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Некоректні символи в імені користувача пошти!",

                        [SupportedCultures.English] =
                            "Invalid characters in the email username!"
                    },

                [DomainErrorCodes.Email.TooLongEmailLocalPart] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Ім'я користувача пошти занадто довге!",

                        [SupportedCultures.English] =
                            "Email local part is too long!"
                    },

                [DomainErrorCodes.Email.TooShortEmailLocalPart] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Ім'я користувача пошти занадто коротке!",

                        [SupportedCultures.English] =
                            "Email local part is too short!"
                    }
            };
    }
}
