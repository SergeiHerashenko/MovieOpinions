using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Application.Abstractions.Security.Hashers
{
    public interface IPasswordHasher
    {
        PasswordHash HashPassword(PlainPassword password);

        bool VerifyPassword(PlainPassword password, string hash);

        void FakeVerifyPassword(PlainPassword password);
    }
}
