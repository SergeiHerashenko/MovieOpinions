using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado.Models
{
    internal sealed class UserData
    {
        public required UserId Id { get; init; }

        public required DateTimeOffset CreatedAt { get; init; }

        public required LoginType LoginType { get; init; }

        public required Login Login { get; init; }

        public required Password Password { get; init; }

        public required Role Role { get; init; }

        public DateTimeOffset? UpdatedAt { get; init; }

        public DateTimeOffset? LastLoginAt { get; init; }

        public required bool IsLoginConfirmed { get; init; }

        public required int FailedLoginAttempts { get; init; }
    }
}
