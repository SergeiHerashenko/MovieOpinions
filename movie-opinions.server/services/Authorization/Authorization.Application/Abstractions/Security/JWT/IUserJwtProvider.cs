using Authorization.Application.Common.Security.Models;

namespace Authorization.Application.Abstractions.Security.JWT
{
    public interface IUserJwtProvider
    {
        string GenerateAccessToken(UserSessionDTO userSessionDTO);
    }
}
