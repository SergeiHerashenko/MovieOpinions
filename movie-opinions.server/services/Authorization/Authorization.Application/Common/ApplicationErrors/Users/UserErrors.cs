using Authorization.Domain.Common.Errors;
using System.Text;

namespace Authorization.Application.Common.ApplicationErrors.Users
{
    public static class UserErrors
    {
        public static Error Exists<TValue>(string loginName)
            => new(ApplicationErrorCodes.UserErrorCode.AlreadyExists,
                   $"User with login {loginName} already exists. Owner: {typeof(TValue).Name}!",
                   ErrorType.Conflict
            );

        public static Error NotFound<TValue>(string login)
            => new(ApplicationErrorCodes.UserErrorCode.NotFound,
                   $"User {login} not found. Owner: {typeof(TValue).Name}!",
                   ErrorType.NotFound
            );

        public static Error InvalidPassword<TValue>(string login)
            => new(ApplicationErrorCodes.UserErrorCode.InvalidPassword,
                   $"User {login} invalid password. Owner: {typeof(TValue).Name}!",
                   ErrorType.InvalidFormat
            );

        public static Error ActiveRestrictions(IEnumerable<string> reasons, DateTimeOffset date)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Ваш акаунт заблоковано. Дата зняття бану: {date}");
            builder.AppendLine();
            builder.AppendLine("Причини:");

            foreach (var reason in reasons)
            {
                builder.AppendLine($"- {reason}");
            }

            return new Error(
                ApplicationErrorCodes.RestrictionsErrorCode.ActiveRestrictions,
                builder.ToString(),
                ErrorType.Forbidden
            );
        }
    }
}
