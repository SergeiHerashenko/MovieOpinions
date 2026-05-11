using Authorization.Domain.Entities;

namespace Authorization.Application.Interfaces.Repositories
{
    public interface IUserRestrictionRepository : IBaseRepository<UserRestriction>
    {
        Task<UserRestriction?> GetActiveBanByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task<IEnumerable<UserRestriction>> GetBansByAdminNicknameAsync(string adminNickname, CancellationToken cancellationToken);

        Task<IEnumerable<UserRestriction>> GetAllBansByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task<UserRestriction?> GetBanByIdAsync(Guid banId, CancellationToken cancellationToken);
    }
}
