using Authorization.Application.Common.Security.Models;

namespace Authorization.Application.Interfaces.Security.JWT
{
    public interface IUserJwtProvider
    {
        string GenerateAccessToken(UserSessionDTO userSessionDTO);
    }
}
