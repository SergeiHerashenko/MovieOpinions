using Authorization.Domain.Users.Enums;

namespace Authorization.Application.Features.Authentication.ConfirmRegistration
{
    public class ConfirmRegistrationResult
    {
        public bool IsSuccess { get; set; }

        public Role Role { get; private set; }

        public string Message { get; private set; }

        private ConfirmRegistrationResult(bool isSuccess, Role role, string message)
        {
            IsSuccess = isSuccess;
            Role = role;
            Message = message;
        }

        public static ConfirmRegistrationResult Success(Role role, string? message = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "Реєстрація успішна!";

            return new(true, role, message);
        }

        public static ConfirmRegistrationResult Failure(string? message = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "Помилка при реєстрації. Спробуйте пізніше!";

            return new(false, Role.Guest, message);
        }
    }
}
