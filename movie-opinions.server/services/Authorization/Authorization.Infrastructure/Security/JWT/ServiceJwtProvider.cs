using Authorization.Infrastructure.Security.JWT.Interfaces;
using Authorization.Infrastructure.Security.JWT.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authorization.Infrastructure.Security.JWT
{
    public class ServiceJwtProvider : IServiceJwtProvider
    {
        private readonly ServiceJwtProviderOptions _options;

        public ServiceJwtProvider(IOptions<ServiceJwtProviderOptions> options)
        {
            _options = options.Value;
        }

        public string GenerateServiceToken(string serviceName, string[] permissions)
        {
            var claims = new List<Claim>()
            {
                new Claim("token_type", "service"),
                new Claim("service", serviceName),
                new Claim("jti", Guid.CreateVersion7().ToString())
            };

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var jwtKey = _options.Key;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
