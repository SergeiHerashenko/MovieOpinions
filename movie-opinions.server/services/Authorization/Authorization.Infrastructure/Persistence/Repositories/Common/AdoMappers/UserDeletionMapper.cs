using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers.Common;
using Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers
{
    internal static class UserDeletionMapper
    {
        public static UserDeletion Map(NpgsqlDataReader reader, UserDeletionOrdinals ordinals)
        {
            var id = UserDeletionId.Restore(reader.GetGuid(ordinals.Id));
            var createdAt = reader.GetFieldValue<DateTimeOffset>(ordinals.CreatedAt);

            var userId = UserId.Restore(reader.GetGuid(ordinals.UserId));

            var login = LoginMapper.Restore<UserDeletion>(
                reader,
                ordinals.LoginType,
                ordinals.Login,
                ordinals.PhoneCountryCode,
                ordinals.EmailDomainPart,
                id.Value
            );

            var reason = DeletionReason.Restore(reader.GetString(ordinals.Reason));

            var restoreUntil = reader.GetFieldValue<DateTimeOffset>(ordinals.RestoreUntil);
            var restoredAt = reader.IsDBNull(ordinals.RestoredAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ordinals.RestoredAt);

            var deletionStatus = EnumMapper.Restore<DeletionStatus>(
                reader.GetString(ordinals.Status),
                nameof(UserDeletion),
                id.Value
            );

            var updatedAt = reader.IsDBNull(ordinals.UpdatedAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ordinals.UpdatedAt);

            return UserDeletion.Restore(id, userId, login, reason, createdAt, restoreUntil, restoredAt, deletionStatus, updatedAt);
        }
    }
}
