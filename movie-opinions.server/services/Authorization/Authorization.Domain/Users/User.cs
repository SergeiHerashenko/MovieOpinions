using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users
{
    public class User : AggregateRoot<UserId, Guid>
    {
        public Login Login { get; private set; }

        public Password Password { get; private set; }

        public Role Role { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public DateTimeOffset? LastLoginAt { get; private set; }

        public string? LastLoginIp { get; private set; }

        public bool IsLoginConfirmed { get; private set; }

        public int FailedLoginAttempts { get; private set; }

        public bool IsBlocked { get; private set; }

        public bool IsDeleted { get; private set; }

        private const int MaxFailedAttempts = 3;
    }
}
