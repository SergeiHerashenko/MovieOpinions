using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.UserPendingRegistrationRepository.Ado.Ordinals
{
    internal readonly struct UserPendingRegistrationOrdinals
    {
        public readonly int Id;

        public readonly int Login;

        public readonly int LoginType;

        public readonly int CountryCode;

        public readonly int EmailDomain;

        public readonly int PasswordHash;

        public readonly int RegistrationToken;

        public readonly int ExpiresAt;

        public readonly int CreatedAt;

        public UserPendingRegistrationOrdinals(NpgsqlDataReader reader)
        {
            Id = reader.GetOrdinal("id");
            Login = reader.GetOrdinal("login");
            LoginType = reader.GetOrdinal("login_type");
            CountryCode = reader.GetOrdinal("country_code");
            EmailDomain = reader.GetOrdinal("email_domain");
            PasswordHash = reader.GetOrdinal("password_hash");
            RegistrationToken = reader.GetOrdinal("registration_token");
            ExpiresAt = reader.GetOrdinal("expires_at");
            CreatedAt = reader.GetOrdinal("created_at");
        }
    }
}
