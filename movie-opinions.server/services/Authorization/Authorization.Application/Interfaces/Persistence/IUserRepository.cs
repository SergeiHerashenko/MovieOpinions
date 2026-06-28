using Authorization.Domain.Users;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Application.Interfaces.Persistence
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetUserByLoginAsync(Login login, CancellationToken cancellationToken = default);

        Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<bool> ExistsByLoginAsync(Login login, CancellationToken cancellationToken = default);
    }
}
