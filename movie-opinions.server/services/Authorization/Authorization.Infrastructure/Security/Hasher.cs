using Authorization.Application.Interfaces.Security;

namespace Authorization.Infrastructure.Security
{
    public class Hasher : IHasher
    {
        public string Hash(string value)
        {
            return BCrypt.Net.BCrypt.HashPassword(value);
        }

        public bool Verify(string value, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(value, hash);
        }
    }
}
