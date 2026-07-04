using Authorization.Application.Common.Security.Models;
using Authorization.Application.Interfaces.Security.JWT;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authorization.Infrastructure.Security.JWT
{
    public class UserJwtProvider : IUserJwtProvider
    {
        private readonly IConfiguration _configuration;
        private const int TIME_LIFE_TOKEN_ACCESS = 15;

        public UserJwtProvider(IConfiguration configuration)
        {
            _configuration = configuration;
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

            return GenerateToken(claims, TIME_LIFE_TOKEN_ACCESS);
        }

        private string GenerateToken(IEnumerable<Claim> claims, int lifeTimeToken)
        {
            // 1. Отримуємо ключ із конфігурації
            var jwtKey = _configuration["Authentication:UserJwt:Key"];

            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
            {
                throw new Exception("Критична помилка: JWT Key занадто короткий або відсутній!");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 2. Створюємо сам токен
            var token = new JwtSecurityToken(
                issuer: _configuration["Authentication:UserJwt:Issuer"],
                audience: _configuration["Authentication:UserJwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(lifeTimeToken),
                signingCredentials: creds
            );

            // 3. Повертаємо серіалізований рядок
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
