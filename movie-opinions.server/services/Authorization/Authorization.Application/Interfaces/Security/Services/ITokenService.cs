using Authorization.Application.Common.Security.Models;
using Authorization.Domain.Results;

namespace Authorization.Application.Interfaces.Security.Services
{
    public interface ITokenService
    {
        Task<Result<TokenResponse>> CreateUserSessionAsync(UserSessionDTO userSessionDTO);
    }
}
