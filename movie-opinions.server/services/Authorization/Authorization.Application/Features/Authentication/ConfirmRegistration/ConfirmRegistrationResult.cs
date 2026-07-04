using Authorization.Application.Common.Security.Models;
using Authorization.Domain.Users.Enums;

namespace Authorization.Application.Features.Authentication.ConfirmRegistration
{
    public class ConfirmRegistrationResult
    {
        public bool IsSuccess { get; set; }

        public Role Role { get; private set; }

        public string Message { get; private set; }

        public TokenResponse? TokenResponse { get; private set; }

        private ConfirmRegistrationResult(bool isSuccess, Role role, string message, TokenResponse? tokens)
        {
            IsSuccess = isSuccess;
            Role = role;
            Message = message;
            TokenResponse = tokens;
        }

        public static ConfirmRegistrationResult Success(Role role, TokenResponse tokens, string? message = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "Реєстрація успішна!";

            return new(true, role, message, tokens);
        }

        public static ConfirmRegistrationResult Failure(string? message = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "Помилка при реєстрації. Спробуйте пізніше!";

            return new(false, Role.Guest, message, null);
        }
    }
}
