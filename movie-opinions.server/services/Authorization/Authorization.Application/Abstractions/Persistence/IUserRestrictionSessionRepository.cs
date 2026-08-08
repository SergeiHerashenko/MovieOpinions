using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Application.Abstractions.Persistence
{
    public interface IUserRestrictionSessionRepository
    {
        Task CreateRestrictionSessionAsync(UserRestrictionSession entity, CancellationToken cancellationToken = default);

        Task UpdateRestrictionSessionAsync(UserRestrictionSession entity, CancellationToken cancellationToken = default);

        Task DeleteRestrictionSessionAsync(UserRestrictionSessionId userRestrictionSessionId, CancellationToken cancellationToken = default);

        Task<UserRestrictionSession?> GetRestrictionSessionByIdAsync(UserRestrictionSessionId userRestrictionSessionId, CancellationToken cancellationToken = default);

        Task<UserRestrictionSession?> GetRestrictionSessionByUserIdAndTypeAsync(UserId userId, RestrictionType type, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<UserRestrictionSession>> GetRestrictionsSessionsByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    }
}
