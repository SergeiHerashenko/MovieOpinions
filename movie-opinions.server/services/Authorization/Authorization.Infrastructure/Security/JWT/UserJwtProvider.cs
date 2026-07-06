using Authorization.Application.Common.Security.Models;
using Authorization.Application.Interfaces.Security.JWT;
using Authorization.Infrastructure.Security.JWT.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authorization.Infrastructure.Security.JWT
{
    public class UserJwtProvider : IUserJwtProvider
    {
        private UserJwtProviderOptions _options;

        public UserJwtProvider(IOptions<UserJwtProviderOptions> options)
        {
            _options = options.Value;
        }

        public string GenerateAccessToken(UserSessionDTO userSessionDTO)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userSessionDTO.UserId.Value.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, userSessionDTO.Login.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, userSessionDTO.Role.ToString()),
            };

            return GenerateToken(claims, _options.AccessTokenLifetimeInMinutes);
        }

        private string GenerateToken(IEnumerable<Claim> claims, int lifeTimeToken)
        {
            // 1. Отримуємо ключ із конфігурації
            if (string.IsNullOrEmpty(_options.Key) || _options.Key.Length < 32)
            {
                throw new Exception("Critical error: JWT key is too short or missing!");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 2. Створюємо сам токен
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(lifeTimeToken),
                signingCredentials: creds
            );

            // 3. Повертаємо серіалізований рядок
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
