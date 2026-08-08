using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers.Common;
using Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals;
using Npgsql;
using System.Text.Json;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers
{
    internal static class UserRestrictionMapper
    {
        public static UserRestriction Map(NpgsqlDataReader reader, UserRestrictionOrdinals ordinals)
        {
            var id = UserRestrictionId.Restore(reader.GetGuid(ordinals.Id));
            var createdAt = reader.GetFieldValue<DateTimeOffset>(ordinals.CreatedAt);

            var userId = UserId.Restore(reader.GetGuid(ordinals.UserId));

            var restrictionRuleJson = reader.GetString(ordinals.RestrictionRule);
            var restrictionRule = RestoreRestrictionRule(restrictionRuleJson);

            var restrictionType = EnumMapper.Restore<RestrictionType>(
                reader.GetString(ordinals.RestrictionType),
                nameof(UserRestriction),
                id.Value
            );

            var reason = reader.GetString(ordinals.Reason);
            var restrictedBy = reader.GetString(ordinals.RestrictedBy);

            var isRevoked = reader.GetBoolean(ordinals.IsRevoked);

            var cancellationDate = reader.IsDBNull(ordinals.CancellationDate) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ordinals.CancellationDate);

            return UserRestriction.Restore(id, userId, restrictionRule, restrictionType, restrictedBy, reason, isRevoked, createdAt, cancellationDate);
        }

        private static RestrictionRule RestoreRestrictionRule(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<RestrictionRule>(json)
                    ?? throw DataConsistencyException.InvalidData(
                        "RestrictionRule json is null.",
                        new Dictionary<string, object>
                        {
                            ["Entity"] = nameof(RestrictionRule)
                        }
                    );
            }
            catch (JsonException ex)
            {
                throw DataConsistencyException.InvalidData(
                    "RestrictionRule json has invalid format.",
                    new Dictionary<string, object>
                    {
                        ["Entity"] = nameof(RestrictionRule)
                    },
                    ex
                );
            }
        }
    }
}
