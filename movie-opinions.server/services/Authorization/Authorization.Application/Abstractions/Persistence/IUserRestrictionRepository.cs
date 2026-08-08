using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;

namespace Authorization.Application.Abstractions.Persistence
{
    public interface IUserRestrictionRepository
    {
        Task CreateRestrictionAsync(UserRestriction entity, CancellationToken cancellationToken = default);

        Task UpdateRestrictionAsync(UserRestriction entity, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<UserRestriction>> GetRestrictionsByIdsAsync(IReadOnlyCollection<UserRestrictionId> userRestrictionIds, CancellationToken cancellationToken = default);
    }
}
