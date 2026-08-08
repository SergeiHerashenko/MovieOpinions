using Authorization.Application.Common.Security.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users;

namespace Authorization.Application.Abstractions.Services
{
    public interface ITokenService
    {
        Result<TokenResponse> CreateUserSessionAsync(User user, CancellationToken cancellationToken = default);
    }
}
