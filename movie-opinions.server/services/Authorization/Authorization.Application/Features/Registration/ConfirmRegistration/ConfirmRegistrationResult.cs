using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Enums;

namespace Authorization.Application.Features.Registration.ConfirmRegistration
{
    public class ConfirmRegistrationResult<TId>
    {
        public bool IsSuccess { get; private set; }

        public TId UserId { get; private set; }

        public Role Role { get; private set; }

        public string Message { get; private set; }

        public string AccessToken { get; private set; }

        public string RefreshToken { get; private set; }

        internal ConfirmRegistrationResult(bool isSuccess, TId userid, Role role, string message, string accessToken, string refreshToken)
        {
            IsSuccess = isSuccess;
            UserId = userid;
            Role = role;
            Message = message;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }

    public static class ConfirmRegistrationResult
    {
        public static ConfirmRegistrationResult<TId> Success<TId>(
            AggregateRootId<TId> aggregateId,
            Role role,
            string accessToken, 
            string refreshToken,
            string? message = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "Реєстрація успішна!";

            return new(true, aggregateId.Value, role, message, accessToken, refreshToken);
        }
    }
}
