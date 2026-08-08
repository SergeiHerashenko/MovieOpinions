using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals
{
    internal readonly struct UserOrdinals
    {
        public readonly int Id;

        public readonly int CreatedAt;

        public readonly int LoginType;

        public readonly int Login;

        public readonly int PhoneCountryCode;

        public readonly int EmailDomainPart;

        public readonly int PasswordHash;

        public readonly int Role;

        public readonly int UpdatedAt;

        public readonly int LastLoginAt;

        public readonly int IsLoginConfirmed;

        public readonly int FailedLoginAttempts;

        public UserOrdinals(NpgsqlDataReader reader)
        {
            Id = reader.GetOrdinal("id");
            CreatedAt = reader.GetOrdinal("created_at");
            LoginType = reader.GetOrdinal("login_type");
            Login = reader.GetOrdinal("login");
            PhoneCountryCode = reader.GetOrdinal("country_code");
            EmailDomainPart = reader.GetOrdinal("email_domain");
            PasswordHash = reader.GetOrdinal("password_hash");
            Role = reader.GetOrdinal("role");
            UpdatedAt = reader.GetOrdinal("updated_at");
            LastLoginAt = reader.GetOrdinal("last_login_at");
            IsLoginConfirmed = reader.GetOrdinal("is_login_confirmed");
            FailedLoginAttempts = reader.GetOrdinal("failed_login_attempts");
        }
    }
}
