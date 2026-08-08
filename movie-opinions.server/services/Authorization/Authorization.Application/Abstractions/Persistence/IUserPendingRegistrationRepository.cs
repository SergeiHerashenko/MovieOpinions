using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;

namespace Authorization.Application.Abstractions.Persistence
{
    public interface IUserPendingRegistrationRepository
    {
        Task<UserPendingRegistration?> GetPendingUserByTokenAsync(RegistrationFlowToken registrationFlowToken, CancellationToken cancellationToken = default);

        Task<UserPendingRegistration?> GetPendingUserByLoginAsync(Login login, CancellationToken cancellationToken = default);

        Task CreatePendingUserAsync(UserPendingRegistration entity, CancellationToken cancellationToken = default);

        Task UpdatePendingUserAsync(UserPendingRegistration entity, CancellationToken cancellationToken = default);

        Task DeletePendingUserAsync(UserPendingRegistrationId userPendingRegistrationId, CancellationToken cancellationToken = default);
    }
}
