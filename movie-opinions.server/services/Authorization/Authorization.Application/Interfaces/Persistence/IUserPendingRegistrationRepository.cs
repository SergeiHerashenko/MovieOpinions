using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;

namespace Authorization.Application.Interfaces.Persistence
{
    public interface IUserPendingRegistrationRepository : IBaseRepository<UserPendingRegistration>
    {
        Task<UserPendingRegistration?> GetByLoginAsync(Login login, CancellationToken cancellationToken = default); 

        Task<UserPendingRegistration?> GetByTokenAsync(RegistrationToken registrationToken, CancellationToken cancellationToken = default);

        Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
    }
}
