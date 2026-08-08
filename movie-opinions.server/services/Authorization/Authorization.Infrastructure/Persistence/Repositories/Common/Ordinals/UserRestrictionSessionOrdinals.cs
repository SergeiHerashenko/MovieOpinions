using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals
{
    internal readonly struct UserRestrictionSessionOrdinals
    {
        public readonly int Id;

        public readonly int CreatedAt;

        public readonly int UserId;

        public readonly int ActiveRestrictionsIds;

        public readonly int RestrictionType;

        public readonly int TotalBlockedMinutes;

        public UserRestrictionSessionOrdinals(NpgsqlDataReader reader)
        {
            Id = reader.GetOrdinal("id");
            CreatedAt = reader.GetOrdinal("created_at");
            UserId = reader.GetOrdinal("user_id");
            ActiveRestrictionsIds = reader.GetOrdinal("active_restrictions_ids");
            RestrictionType = reader.GetOrdinal("restriction_type");
            TotalBlockedMinutes = reader.GetOrdinal("total_blocked_minutes");
        }
    }
}
