using Authorization.Domain.Results;
using Authorization.Domain.Users;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.Abstractions.Services.UserPassword
{
    public interface IUserPasswordAttemptService
    {
        Task<Result<User>> ProcessAsync(
            UserId userId,
            string rawPassword,
            CancellationToken cancellationToken = default);

        Task<Result<User>> ProcessAsync(
            Login login,
            string rawPassword,
            CancellationToken cancellationToken = default);
    }
}
