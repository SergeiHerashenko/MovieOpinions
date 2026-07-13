using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersDeletion;

namespace Authorization.Application.Interfaces.Persistence
{
    public interface IUserDeletedRepository : IBaseRepository<UserDeletion>
    {
        Task<UserDeletion?> GetDeletionUserById(UserId userId);
    }
}
