using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals
{
    internal readonly struct UserDeletionOrdinals
    {
        public readonly int Id;

        public readonly int CreatedAt;

        public readonly int UserId;

        public readonly int LoginType;

        public readonly int Login;

        public readonly int PhoneCountryCode;

        public readonly int EmailDomainPart;

        public readonly int Reason;

        public readonly int RestoreUntil;

        public readonly int RestoredAt;

        public readonly int Status;

        public readonly int UpdatedAt;

        public UserDeletionOrdinals(NpgsqlDataReader reader)
        {
            Id = reader.GetOrdinal("id");
            CreatedAt = reader.GetOrdinal("created_at");
            UserId = reader.GetOrdinal("user_id");
            LoginType = reader.GetOrdinal("login_type");
            Login = reader.GetOrdinal("login");
            PhoneCountryCode = reader.GetOrdinal("country_code");
            EmailDomainPart = reader.GetOrdinal("email_domain");
            Reason = reader.GetOrdinal("reason");
            RestoreUntil = reader.GetOrdinal("restore_until");
            RestoredAt = reader.GetOrdinal("restored_at");
            Status = reader.GetOrdinal("status");
            UpdatedAt = reader.GetOrdinal("updated_at");
        }
    }
}
