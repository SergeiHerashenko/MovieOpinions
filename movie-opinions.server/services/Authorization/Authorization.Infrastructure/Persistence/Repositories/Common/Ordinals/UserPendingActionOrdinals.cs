using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals
{
    internal readonly struct UserPendingActionOrdinals
    {
        public readonly int Id;

        public readonly int CreatedAt;

        public readonly int UserId;

        public readonly int ConfirmationToken;

        public readonly int UserActionType;

        public readonly int LoginType;

        public readonly int UserActionData;

        public readonly int ExpiresAt;

        public readonly int ConfirmationTime;

        public readonly int ExpiredAt;

        public readonly int CancelledTime;

        public readonly int Status;

        public UserPendingActionOrdinals(NpgsqlDataReader reader)
        {
            Id = reader.GetOrdinal("id");
            CreatedAt = reader.GetOrdinal("created_at");
            UserId = reader.GetOrdinal("user_id");
            ConfirmationToken = reader.GetOrdinal("confirmation_token");
            UserActionType = reader.GetOrdinal("action_type");
            LoginType = reader.GetOrdinal("login_type");
            UserActionData = reader.GetOrdinal("action_data");
            ExpiresAt = reader.GetOrdinal("expires_at");
            ConfirmationTime = reader.GetOrdinal("confirmation_time");
            ExpiredAt = reader.GetOrdinal("expired_at");
            CancelledTime = reader.GetOrdinal("cancelled_time");
            Status = reader.GetOrdinal("status");
        }
    }
}
