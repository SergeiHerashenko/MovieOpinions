using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects;
using Authorization.Domain.ValueObjects.Login;

namespace Authorization.Application.Interfaces.Services
{
    public interface IPendingRegistrationService
    {
        Task<UserPendingRegistration> CreateOrRefreshAsync(Login login, Password password, CancellationToken cancellationToken);
    }
}
