using Authorization.Application.Interfaces.Errors;
using Authorization.Domain.Errors;

namespace Authorization.Infrastructure.Errors
{
    public class ErrorMessageProvider : IErrorMessageProvider
    {
        private static readonly Dictionary<string, Dictionary<string, string>> _messages = new()
        {
            // LOGIN
            [ErrorCodes.LoginError.Invalid] = new()
            {
                ["uk"] = "Невірний формат логіну",
                ["en"] = "Invalid login format"
            },
            [ErrorCodes.LoginError.InvalidEmail] = new()
            {
                ["uk"] = "Невірний формат email",
                ["en"] = "Invalid email format"
            },
            [ErrorCodes.LoginError.InvalidPhone] = new()
            {
                ["uk"] = "Невірний формат телефону",
                ["en"] = "Invalid phone format"
            },
            [ErrorCodes.LoginError.Empty] = new()
            {
                ["uk"] = "Логін є обов'язковим",
                ["en"] = "Login is required"
            },

            // PASSWORD
            [ErrorCodes.PasswordError.Empty] = new()
            {
                ["uk"] = "Пароль є обов'язковим",
                ["en"] = "Password is required"
            },

            // ACCOUNT STATUS
            [ErrorCodes.AccountStatusError.Blocked] = new()
            {
                ["uk"] = "Користувач заблокований",
                ["en"] = "User is blocked"
            },
            [ErrorCodes.AccountStatusError.Deleted] = new()
            {
                ["uk"] = "Користувач видалений",
                ["en"] = "User is deleted"
            },

            // GENERAL
            [ErrorCodes.GeneralError.OperationNotAllowed] = new()
            {
                ["uk"] = "Операція заборонена",
                ["en"] = "Operation is not allowed"
            },

            // USER PENDING CHANGE
            [ErrorCodes.UserPendingChangeError.InvalidConfirmToken] = new()
            {
                ["uk"] = "Невірний токен підтвердження",
                ["en"] = "Invalid confirmation token"
            },

            // TOKEN
            [ErrorCodes.TokenError.TokenEmpty] = new()
            {
                ["uk"] = "Токен не може бути порожнім",
                ["en"] = "Token cannot be empty"
            },
            [ErrorCodes.TokenError.TokenInvalid] = new()
            {
                ["uk"] = "Токен недійсний",
                ["en"] = "Token is invalid"
            },
            [ErrorCodes.TokenError.TokenExpired] = new()
            {
                ["uk"] = "Термін дії токена завершився",
                ["en"] = "Token has expired"
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
