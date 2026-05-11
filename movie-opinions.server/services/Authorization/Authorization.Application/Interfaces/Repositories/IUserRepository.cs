using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects.Login;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetUserByLoginAsync(Login login, CancellationToken cancellationToken);

        Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);

        Task<bool> ExistsByLoginAsync(Login login, CancellationToken cancellationToken);
    }
}
