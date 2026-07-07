using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersRestriction;

namespace Authorization.Application.Interfaces.Persistence
{
    public interface IUserRestrictionSessionRepository : IBaseRepository<UserRestrictionSession>
    {
        Task<UserRestrictionSession?> GetSessionRestrictionByLoginAsync(Login login);
    }
}
