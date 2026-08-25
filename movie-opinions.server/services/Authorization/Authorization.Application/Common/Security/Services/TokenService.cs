using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Security.JWT;
using Authorization.Application.Abstractions.Services;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Application.Common.Security.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;

namespace Authorization.Application.Common.Security.Services
{
    public class TokenService : ITokenService
    {
        private readonly IUserJwtProvider _userJwtProvider;
        private readonly IUserContext _userContext;
        private readonly IClock _clock;

        public TokenService(
            IUserJwtProvider userJwtProvider,
            IUserContext userContext,
            IClock clock)
        {
            _userJwtProvider = userJwtProvider;
            _userContext = userContext;
            _clock = clock;
        }

        public Result<TokenResponse> CreateUserSession(User user, CancellationToken cancellationToken = default)
        {
            var userSessionDTO = UserSessionDTO.Create(
                user.Id, 
                user.Login,
                user.Role
            );

            var accessToken = _userJwtProvider.GenerateAccessToken(userSessionDTO);

            var ipAddressResult = IpAddress.Create(_userContext.GetIpAddress());

            if (ipAddressResult.IsFailure)
                return Result<TokenResponse>.Failure(ipAddressResult.Errors);

            var ipAddress = ipAddressResult.Value;

            var createTokenResult = user.CreateRefreshToken(
                _userContext.InfoDevice(),
                ipAddress,
                _clock.UtcNow,
                _userContext.GetLocation()
            );

            if (createTokenResult.IsFailure)
                return Result<TokenResponse>.Failure(createTokenResult.Errors);

            var token = new TokenResponse()
            {
                AccessToken = accessToken,
                UserRefreshToken = createTokenResult.Value
            };

            return Result<TokenResponse>.Success(token);
        }
    }
}
