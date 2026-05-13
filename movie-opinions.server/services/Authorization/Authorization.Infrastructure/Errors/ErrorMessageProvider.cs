using Authorization.Application.Errors;
using Authorization.Application.Interfaces.Errors;
using Authorization.Domain.Errors;

namespace Authorization.Infrastructure.Errors
{
    public class ErrorMessageProvider : IErrorMessageProvider
    {
        private static readonly Dictionary<string, Dictionary<string, string>> _messages = new()
        {
            // LOGIN
            [DomainErrorCodes.LoginError.Invalid] = new()
            {
                ["uk"] = "Невірний формат логіну",
                ["en"] = "Invalid login format"
            },
            [DomainErrorCodes.LoginError.InvalidEmail] = new()
            {
                ["uk"] = "Невірний формат email",
                ["en"] = "Invalid email format"
            },
            [DomainErrorCodes.LoginError.InvalidPhone] = new()
            {
                ["uk"] = "Невірний формат телефону",
                ["en"] = "Invalid phone format"
            },
            [DomainErrorCodes.LoginError.Empty] = new()
            {
                ["uk"] = "Логін є обов'язковим",
                ["en"] = "Login is required"
            },

            // PASSWORD
            [DomainErrorCodes.PasswordError.Empty] = new()
            {
                ["uk"] = "Пароль є обов'язковим",
                ["en"] = "Password is required"
            },

            // ACCOUNT STATUS
            [DomainErrorCodes.AccountStatusError.Blocked] = new()
            {
                ["uk"] = "Користувач заблокований",
                ["en"] = "User is blocked"
            },
            [DomainErrorCodes.AccountStatusError.Deleted] = new()
            {
                ["uk"] = "Користувач видалений",
                ["en"] = "User is deleted"
            },

            // GENERAL
            [DomainErrorCodes.GeneralError.OperationNotAllowed] = new()
            {
                ["uk"] = "Операція заборонена",
                ["en"] = "Operation is not allowed"
            },

            // USER PENDING CHANGE
            [DomainErrorCodes.UserPendingChangeError.InvalidConfirmToken] = new()
            {
                ["uk"] = "Невірний токен підтвердження",
                ["en"] = "Invalid confirmation token"
            },

            // TOKEN
            [DomainErrorCodes.TokenError.TokenEmpty] = new()
            {
                ["uk"] = "Токен не може бути порожнім",
                ["en"] = "Token cannot be empty"
            },
            [DomainErrorCodes.TokenError.TokenInvalid] = new()
            {
                ["uk"] = "Токен недійсний",
                ["en"] = "Token is invalid"
            },
            [DomainErrorCodes.TokenError.TokenExpired] = new()
            {
                ["uk"] = "Термін дії токена завершився",
                ["en"] = "Token has expired"
            },

            // Infrastructure Layer
            [InfrastructureErrorCodes.RateLimitError.TooManyAttempts] = new()
            {
                ["uk"] = "Занадто багато спроб. Спробуйте пізніше",
                ["en"] = "Too many attempts"
            },

            // Application Layer
            [ApplicationErrorCodes.ErrorUser.UserAlreadyExists] = new()
            {
                ["uk"] = "Користувач з таким логіном вже існує",
                ["en"] = "A user with this login already exists."
            }
        };

        public string GetMessage(string errorCode, string culture)
        {
            var lang = culture.StartsWith("uk") ? "uk" : "en";

            if(_messages.TryGetValue(errorCode, out var message))
            {
                return message.TryGetValue(lang, out var msg)
                    ? msg
                    : message["en"];
            }

            return lang == "uk" ? "Виникла внутрішня помилка сервера" : "Unexpected error";
        }
    }
}
