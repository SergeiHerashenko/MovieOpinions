using Authorization.Domain.Entities;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserPendingAccountChangesRepository : IBaseRepository<UserPendingChange>
    {
        Task<UserPendingChange?> GetPendingChangesAsync(Guid id, CancellationToken cancellationToken);
    }
}
