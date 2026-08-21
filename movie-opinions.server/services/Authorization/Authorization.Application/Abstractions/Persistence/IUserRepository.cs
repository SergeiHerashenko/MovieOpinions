using Authorization.Domain.Users;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.Abstractions.Persistence
{
    public interface IUserRepository
    {
        Task CreateUserAsync(User entity, CancellationToken cancellationToken = default);

        Task UpdateUserAsync(User entity, CancellationToken cancellationToken = default);

        Task<User?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken = default);

        Task<User?> GetUserByIdForUpdateAsync(UserId userId, CancellationToken cancellationToken = default);

        Task<User?> GetUserByLoginAsync(Login login, CancellationToken cancellationToken = default);

        Task<bool> ExistsUserByLoginAsync(Login login, CancellationToken cancellationToken = default);
    }
}
