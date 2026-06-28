using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration;

namespace Authorization.Application.Interfaces.Persistence
{
    public interface IUserPendingRegistrationRepository : IBaseRepository<UserPendingRegistration>
    {
        Task<UserPendingRegistration?> GetByLoginAsync(Login login, CancellationToken cancellationToken = default); 
    }
}
