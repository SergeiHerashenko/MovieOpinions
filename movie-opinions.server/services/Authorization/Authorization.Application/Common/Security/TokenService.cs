using Authorization.Application.Common.Security.Models;
using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Security.JWT;
using Authorization.Domain.Results;
using Authorization.Domain.UsersRefreshToken;

namespace Authorization.Application.Common.Security
{
    public class TokenService : ITokenService
    {
        private readonly IUserJwtProvider _userJwtProvider;
        private readonly IUserContext _userContext;

        private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;

        public TokenService(
            IUserJwtProvider userJwtProvider,
            IUserContext userContext,
            IUserRefreshTokenRepository userRefreshToken)
        {
            _userJwtProvider = userJwtProvider;
            _userContext = userContext;
            _userRefreshTokenRepository = userRefreshToken;
        }

        public async Task<Result<TokenResponse>> CreateUserSessionAsync(UserSessionDTO userSessionDTO)
        {
            var accessToken = _userJwtProvider.GenerateAccessToken(userSessionDTO);

            var userToken = UserRefreshToken.Create(
                userSessionDTO.UserId,
                _userContext.DeviceInfo,
                userSessionDTO.IpAddress,
                _userContext.GetLocation()
            );

            if (userToken.IsFailure)
                return Result<TokenResponse>.Failure(userToken.Errors);

            var saveToken = await _userRefreshTokenRepository.CreateAsync(userToken.Value);

            var tokens = new TokenResponse()
            {
                AccessToken = accessToken,
                RefreshToken = saveToken.RefreshToken.Value,
            };

            return Result<TokenResponse>.Success(tokens);
        }
    }
}
