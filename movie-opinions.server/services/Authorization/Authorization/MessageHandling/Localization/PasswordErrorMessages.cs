using Authorization.Application.Common.Errors;
using Authorization.Domain.Common.Errors;

namespace Authorization.MessageHandling.Localization
{
    internal class PasswordErrorMessages
    {
        public static IReadOnlyDictionary<
            string,
            IReadOnlyDictionary<string, string>> Values
        { get; } =
            new Dictionary<
                string,
                IReadOnlyDictionary<string, string>>
            {
                [DomainErrorCodes.Password.EmptyPlainPassword] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] = 
                            "Пароль не може бути порожнім!",

                        [SupportedCultures.English] = 
                            "Password cannot be empty!"
                    },

                [DomainErrorCodes.Password.MissingLowercaseLetterPlainPassword] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Пароль повинен містити хоча б одну маленьку літеру!",

                        [SupportedCultures.English] =
                            "Password must contain at least one lowercase letter!"
                    },

                [DomainErrorCodes.Password.MissingUppercaseLetterPlainPassword] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Пароль повинен містити хоча б одну велику літеру!",

                        [SupportedCultures.English] =
                            "Password must contain at least one uppercase letter!"
                    },

                [DomainErrorCodes.Password.NoContainNumber] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Пароль повинен містити хоча б одну цифру!",

                        [SupportedCultures.English] =
                            "Password must contain at least one digit!"
                    },

                [DomainErrorCodes.Password.TooLongPlainPassword] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Пароль не повинен перевищувати 64 символів!",

                        [SupportedCultures.English] =
                            "Password must not exceed 64 characters!"
                    },

                [DomainErrorCodes.Password.TooShortPlainPassword] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] = 
                            "Довжина паролю повинна бути не менше 8 символів!",

                        [SupportedCultures.English] = 
                            "The password must be at least 8 characters long!"
                    },

                [ApplicationErrorCodes.RegistrationError.ConfirmPasswordRequired] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Підтвердження пароля обов'язкове!",

                        [SupportedCultures.English] =
                            "Password confirmation is required!"
                    },

                [ApplicationErrorCodes.RegistrationError.PasswordsDoNotMatch] =
                    new Dictionary<string, string>
                    {
                        [SupportedCultures.Ukrainian] =
                            "Паролі не співпадають!",

                        [SupportedCultures.English] =
                            "Passwords do not match!"
                    }
            };
    }
}
