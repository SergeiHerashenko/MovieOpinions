using Authorization.Domain.Entities;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserDeletionRepository : IBaseRepository<UserDeletion>
    {
        Task<UserDeletion?> GetUserDeletionsByIdAsync(Guid userId, CancellationToken cancellationToken);

        Task<UserDeletion?> GetUserDeletionsByLoginAsync(string login, CancellationToken cancellationToken);
    }
}
