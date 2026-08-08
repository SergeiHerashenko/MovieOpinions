using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals
{
    internal readonly struct UserRefreshTokenOrdinals
    {
        public readonly int Id;

        public readonly int CreatedAt;

        public readonly int UserId;

        public readonly int RefreshToken;

        public readonly int DeviceInfo;

        public readonly int IpAddress;

        public readonly int City;

        public readonly int TokenStatus;

        public readonly int ExpiresAt;

        public readonly int ConsumedAt;

        public readonly int RevokedAt;

        public UserRefreshTokenOrdinals(NpgsqlDataReader reader)
        {
            Id = reader.GetOrdinal("id");
            CreatedAt = reader.GetOrdinal("created_at");
            UserId = reader.GetOrdinal("user_id");
            RefreshToken = reader.GetOrdinal("refresh_token");
            DeviceInfo = reader.GetOrdinal("device_info");
            IpAddress = reader.GetOrdinal("ip_address");
            City = reader.GetOrdinal("city");
            TokenStatus = reader.GetOrdinal("token_status");
            ExpiresAt = reader.GetOrdinal("expires_at");
            ConsumedAt = reader.GetOrdinal("consumed_at");
            RevokedAt = reader.GetOrdinal("revoked_at");
        }
    }
}
