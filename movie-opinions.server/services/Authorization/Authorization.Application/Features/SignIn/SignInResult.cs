using Authorization.Application.Common.Security.Models;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Enums;

namespace Authorization.Application.Features.SignIn
{
    public class SignInResult<TId>
    {
        public bool IsSuccess { get; private set; }

        public TId UserId { get; private set; }

        public Role Role { get; private set; }

        public string Message { get; private set; }

        public string AccessToken { get; private set; }

        public string RefreshToken { get; private set; }

        internal SignInResult(bool isSuccess, TId userId, Role role, string message, string accessToken, string refreshToken)
        {
            IsSuccess = isSuccess;
            UserId = userId;
            Role = role;
            Message = message;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }

    public static class SignInResult
    {
        public static SignInResult<TId> Success<TId>(
            AggregateRootId<TId> aggregateId,
            Role role,
            string accessToken,
            string refreshToken,
            string? message = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "Вхід успішний!";

            return new(true, aggregateId.Value, role, message, accessToken, refreshToken);
        }
    }
}
