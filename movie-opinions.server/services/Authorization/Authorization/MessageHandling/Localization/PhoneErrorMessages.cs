using Authorization.Domain.Common.Errors;

namespace Authorization.MessageHandling.Localization
{
    internal static class PhoneErrorMessages
    {
        public static IReadOnlyDictionary<
            string,
            IReadOnlyDictionary<string, string>> Values
        { get; } =
            new Dictionary<
                string,
                IReadOnlyDictionary<string, string>>
            {
                [DomainErrorCodes.Phone.EmptyPhoneCountryCode] = 
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Потрібно вказати код країни!",

                        [SupportedCultures.English] =
                            "Country code is required!"
                    },

                [DomainErrorCodes.Phone.InvalidFormatPhoneCountryCode] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Некоректний формат коду країни!",

                        [SupportedCultures.English] =
                            "Invalid country code format!"
                    },

                [DomainErrorCodes.Phone.TooLongPhoneCountryCode] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Код країни занадто довгий!",

                        [SupportedCultures.English] =
                            "Country code is too long!"
                    },

                [DomainErrorCodes.Phone.TooShortPhoneCountryCode] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Код країни занадто короткий!",

                        [SupportedCultures.English] =
                            "Country code is too short!"
                    },

                [DomainErrorCodes.Phone.EmptyPhoneNationalNumber] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Вкажіть номер телефону!",

                        [SupportedCultures.English] =
                            "Phone number is required!"
                    },

                [DomainErrorCodes.Phone.InvalidFormatPhoneNationalNumber] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Некоректний формат номера телефону!",

                        [SupportedCultures.English] =
                            "Invalid phone number format!"
                    },

                [DomainErrorCodes.Phone.TooLongPhoneNationalNumber] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Номер телефону занадто довгий!",

                        [SupportedCultures.English] =
                            "Phone number is too long!"
                    },

                [DomainErrorCodes.Phone.TooShortPhoneNationalNumber] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Номер телефону занадто короткий!",

                        [SupportedCultures.English] =
                            "Phone number is too short!"
                    },

                [DomainErrorCodes.Phone.NotAllowedPhone] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Цей номер телефону заборонений або недоступний!",

                        [SupportedCultures.English] =
                            "This phone number is not allowed!"
                    }
            };
    }
}
