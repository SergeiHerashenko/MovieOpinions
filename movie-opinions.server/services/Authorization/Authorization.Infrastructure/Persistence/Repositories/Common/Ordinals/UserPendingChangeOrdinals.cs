using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals
{
    internal readonly struct UserPendingChangeOrdinals
    {
        public readonly int Id;

        public readonly int CreatedAt;

        public readonly int UserId;

        public readonly int ConfirmationToken;

        public readonly int UserChangeType;

        public readonly int LoginType;

        public readonly int UserChangeData;

        public readonly int ExpiresAt;

        public readonly int ConfirmationTime;

        public readonly int ExpiredAt;

        public readonly int Status;

        public UserPendingChangeOrdinals(NpgsqlDataReader reader)
        {
            Id = reader.GetOrdinal("id");
            CreatedAt = reader.GetOrdinal("created_at");
            UserId = reader.GetOrdinal("user_id");
            ConfirmationToken = reader.GetOrdinal("confirmation_token");
            UserChangeType = reader.GetOrdinal("change_type");
            LoginType = reader.GetOrdinal("login_type");
            UserChangeData = reader.GetOrdinal("change_data");
            ExpiresAt = reader.GetOrdinal("expires_at");
            ConfirmationTime = reader.GetOrdinal("confirmation_time");
            ExpiredAt = reader.GetOrdinal("expired_at");
            Status = reader.GetOrdinal("status");
        }
    }
}
