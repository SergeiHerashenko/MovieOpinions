using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers.Common;
using Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals;
using Npgsql;
using System.Text.Json;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers
{
    internal static class UserRestrictionSessionMapper
    {
        public static UserRestrictionSession Map(NpgsqlDataReader reader, UserRestrictionSessionOrdinals ordinals)
        {
            var id = UserRestrictionSessionId.Restore(reader.GetGuid(ordinals.Id));
            var createdAt = reader.GetFieldValue<DateTimeOffset>(ordinals.CreatedAt);

            var userId = UserId.Restore(reader.GetGuid(ordinals.UserId));

            var activeRestrictionIdsJson = reader.GetString(ordinals.ActiveRestrictionsIds);
            var activeRestrictionIds = RestoreRestrictionIds(activeRestrictionIdsJson);

            var restrictionType = EnumMapper.Restore<RestrictionType>(
                reader.GetString(ordinals.RestrictionType),
                nameof(UserRestrictionSession),
                id.Value
            );

            var totalBlockedMinutes = reader.GetInt32(ordinals.TotalBlockedDays);

            return UserRestrictionSession.Restore(id, userId, activeRestrictionIds, restrictionType, totalBlockedMinutes, createdAt);
        }

        private static IReadOnlyList<UserRestrictionId> RestoreRestrictionIds(string json)
        {
            var metadata = new Dictionary<string, object>
            {
                ["Entity"] = nameof(UserRestrictionSession),
                ["Property"] = "ActiveRestrictionsIds"
            };

            List<Guid> restrictionIds;

            try
            {
                restrictionIds = JsonSerializer.Deserialize<List<Guid>>(json)
                    ?? throw DataConsistencyException.InvalidData(
                        "Restriction ids json is null!",
                        metadata
                    );
            }
            catch (JsonException ex)
            {
                throw DataConsistencyException.InvalidData(
                        "Restriction ids json has invalid format.",
                        metadata,
                        ex);
            }

            if (restrictionIds.Count == 0)
                throw DataConsistencyException.NotConsistentState(
                        $"Restriction session contains no restriction ids!",
                        metadata);

            if (restrictionIds.Distinct().Count() != restrictionIds.Count)
                throw DataConsistencyException.NotConsistentState(
                        "Restriction session contains duplicate restriction ids!",
                        metadata);

            return restrictionIds
                .Select(UserRestrictionId.Restore)
                .ToList();
        }
    }
}
