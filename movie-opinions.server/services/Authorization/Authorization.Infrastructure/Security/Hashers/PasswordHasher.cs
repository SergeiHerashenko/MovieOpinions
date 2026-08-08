using Authorization.Application.Abstractions.Security.Hashers;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Infrastructure.Security.Hashers
{
    public class PasswordHasher : IPasswordHasher
    {
        private const string FakeHash = "$2a$12$YbA4m7b2Eo3QJkG6o2R8gO5XQnK5QbG3Jk7j0Qv0J9Xj6R2lM9J5K";

        public void FakeVerifyPassword(PlainPassword password)
        {
            _ = BCrypt.Net.BCrypt.Verify(password.Value, FakeHash);
        }

        public PasswordHash HashPassword(PlainPassword password)
        {
            return new PasswordHash(BCrypt.Net.BCrypt.HashPassword(password.Value));
        }

        public bool VerifyPassword(PlainPassword password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password.Value, hash);
        }
    }
}
