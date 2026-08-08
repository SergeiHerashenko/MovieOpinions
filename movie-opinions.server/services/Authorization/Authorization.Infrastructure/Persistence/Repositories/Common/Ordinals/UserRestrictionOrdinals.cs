using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals
{
    internal readonly struct UserRestrictionOrdinals
    {
        public readonly int Id;

        public readonly int CreatedAt;

        public readonly int UserId;

        public readonly int RestrictionRule;

        public readonly int RestrictionType;

        public readonly int Reason;

        public readonly int RestrictedBy;

        public readonly int IsRevoked;

        public readonly int CancellationDate;

        public UserRestrictionOrdinals(NpgsqlDataReader reader)
        {
            Id = reader.GetOrdinal("Id");
            CreatedAt = reader.GetOrdinal("created_at");
            UserId = reader.GetOrdinal("user_id");
            RestrictionRule = reader.GetOrdinal("restriction_rule");
            RestrictionType = reader.GetOrdinal("restriction_type");
            Reason = reader.GetOrdinal("reason");
            RestrictedBy = reader.GetOrdinal("restricted_by");
            IsRevoked = reader.GetOrdinal("is_revoked");
            CancellationDate = reader.GetOrdinal("cancellation_date");
        }
    }
}
