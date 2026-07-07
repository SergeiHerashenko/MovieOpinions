using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersRestriction;
using Authorization.Domain.UsersRestriction.ValueObjects;

namespace Authorization.Application.Interfaces.Persistence
{
    public interface IUserRestrictionRepository : IBaseRepository<UserRestriction>
    {
        Task<UserRestriction?> GetRestrictionByLoginAsync(Login login);

        Task<UserRestriction?> GetRestrictionByIdAsync(UserRestrictionId userRestrictionId);

        Task<IEnumerable<UserRestriction>?> GetRestrictionsAsync(IEnumerable<UserRestrictionId> userRestrictionIds);
    }
}
