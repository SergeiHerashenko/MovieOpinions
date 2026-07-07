using Authorization.Application.Common.Security.Models;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Enums;

namespace Authorization.Application.Features.Authentication.SignIn
{
    public class SignInResult<TId>
    {
        public bool IsSuccess { get; private set; }

        public TId UserId { get; private set; }

        public Role Role { get; private set; }

        public string Message { get; private set; }

        public TokenResponse? TokenResponse { get; private set; }

        internal SignInResult(bool isSuccess, TId userId, Role role, string message, TokenResponse? tokenResponse)
        {
            IsSuccess = isSuccess;
            UserId = userId;
            Role = role;
            Message = message;
            TokenResponse = tokenResponse;
        }
    }

    public static class SignInResult
    {
        public static SignInResult<TId> Success<TId>(
            AggregateRootId<TId> aggregateId,
            Role role,
            TokenResponse tokens,
            string? message = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "Вхід успішний!";

            return new(true, aggregateId.Value, role, message, tokens);
        }

        public static SignInResult<TId> Failure<TId>(string? message = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "Помилка при вході! Спробуйте пізніше!";

            return new(false, default!, Role.Guest, message, null);
        }
    }
}
