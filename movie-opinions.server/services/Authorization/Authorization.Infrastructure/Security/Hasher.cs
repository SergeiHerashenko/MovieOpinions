using Authorization.Application.Interfaces.Security;

namespace Authorization.Infrastructure.Security
{
    public class Hasher : IHasher
    {
        private const string FakeHash = "$2a$12$YbA4m7b2Eo3QJkG6o2R8gO5XQnK5QbG3Jk7j0Qv0J9Xj6R2lM9J5K";

        public string Hash(string value)
        {
            return BCrypt.Net.BCrypt.HashPassword(value);
        }

        public bool Verify(string value, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(value, hash);
        }

        public void FakeVerify(string password)
        {
            _ = BCrypt.Net.BCrypt.Verify(password, FakeHash);
        }
    }
}
