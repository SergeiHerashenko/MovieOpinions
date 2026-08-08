using Authorization.Domain.Users.Entities.UsersPendingChange;
using Authorization.Domain.Users.Entities.UsersPendingChange.Changes;
using Authorization.Domain.Users.Entities.UsersPendingChange.Enums;
using Authorization.Domain.Users.Entities.UsersPendingChange.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;
using Authorization.Domain.Users.ValueObjects.PhoneUser;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers.Common;
using Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals;
using Npgsql;
using System.Text.Json;

namespace Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers
{
    internal static class UserPendingChangeMapper
    {
        private const string UserChangeDataProperty = "UserChangeData";

        public static UserPendingChange Map(NpgsqlDataReader reader, UserPendingChangeOrdinals ordinals)
        {
            var id = UserPendingChangeId.Restore(reader.GetGuid(ordinals.Id));
            var createdAt = reader.GetFieldValue<DateTimeOffset>(ordinals.CreatedAt);

            var userId = UserId.Restore(reader.GetGuid(ordinals.UserId));

            var confirmationToken = ConfirmationToken.Restore(reader.GetString(ordinals.ConfirmationToken));

            var changeType = EnumMapper.Restore<UserChangeType>(
                reader.GetString(ordinals.UserChangeType),
                nameof(UserPendingChange),
                id.Value
            );

            var userChangeDataJson = reader.GetString(ordinals.UserChangeData);

            UserChange change = changeType switch
            {
                UserChangeType.PasswordChange => RestorePasswordChange(userChangeDataJson, id.Value),
                UserChangeType.LoginChange => RestoreLoginChange(reader, ordinals.LoginType, id.Value, userChangeDataJson),

                _ => throw DataConsistencyException.UnknownType(
                    $"Unknown type for {nameof(UserChangeType)}",
                    new Dictionary<string, object>
                    {
                        ["UserChangeType"] = changeType,
                        ["Entity"] = nameof(UserPendingChange),
                        ["Id"] = id.Value
                    }
                )
            };

            var expiresAt = reader.GetFieldValue<DateTimeOffset>(ordinals.ExpiresAt);

            var confirmationTime = reader.IsDBNull(ordinals.ConfirmationTime) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ordinals.ConfirmationTime);
            var expiredAt = reader.IsDBNull(ordinals.ExpiredAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ordinals.ExpiredAt);

            var status = EnumMapper.Restore<ChangeStatus>(
                reader.GetString(ordinals.Status),
                nameof(UserPendingChange),
                id.Value
            );

            return UserPendingChange.Restore(id, userId, confirmationToken, change, expiresAt, confirmationTime, expiredAt, status, createdAt);
        }

        private static LoginChange RestoreLoginChange<T>(NpgsqlDataReader reader, int loginTypeOrd, T entityId, string json)
            where T : notnull
        {
            var loginType = EnumMapper.Restore<LoginType>(
                reader.GetString(loginTypeOrd),
                nameof(UserPendingChange),
                entityId
            );

            return loginType switch
            {
                LoginType.Email => LoginChange.Restore(RestoreEmail(json, entityId)),
                LoginType.Phone => LoginChange.Restore(RestorePhone(json, entityId)),
                _ => throw DataConsistencyException.UnknownType(
                    $"Unknown type for {nameof(LoginType)}",
                    new Dictionary<string, object>
                    {
                        ["LoginType"] = loginType,
                        ["Entity"] = nameof(UserPendingChange),
                        ["EntityId"] = entityId
                    }
                )
            };
        }

        private static Email RestoreEmail(string json, object entityId)
        {
            try
            {
                return JsonSerializer.Deserialize<Email>(json)
                    ?? throw DataConsistencyException.InvalidData(
                        "UserChangeData contains null email data.",
                        BuildContext(entityId, UserChangeType.LoginChange, LoginType.Email)
                    );
            }
            catch (JsonException ex)
            {
                throw DataConsistencyException.InvalidData(
                    "UserChangeData contains invalid email data.",
                    BuildContext(entityId, UserChangeType.LoginChange, LoginType.Email),
                    ex
                );
            }
        }

        private static Phone RestorePhone(string json, object entityId)
        {
            try
            {
                return JsonSerializer.Deserialize<Phone>(json)
                    ?? throw DataConsistencyException.InvalidData(
                        "UserChangeData contains null phone data.",
                        BuildContext(entityId, UserChangeType.LoginChange, LoginType.Phone)
                    );
            }
            catch (JsonException ex)
            {
                throw DataConsistencyException.InvalidData(
                    "UserChangeData contains invalid phone data.",
                    BuildContext(entityId, UserChangeType.LoginChange, LoginType.Phone),
                    ex
                );
            }
        }

        private static PasswordChange RestorePasswordChange<T>(string json, T entityId)
            where T : notnull
        {
            try
            {
                var passwordHash = JsonSerializer.Deserialize<PasswordHash>(json)
                    ?? throw DataConsistencyException.InvalidData(
                        "UserChangeData contains null password data.",
                        BuildContext(entityId, UserChangeType.PasswordChange)
                    );

                return PasswordChange.Restore(passwordHash);
            }
            catch (JsonException ex)
            {
                throw DataConsistencyException.InvalidData(
                    "UserChangeData contains invalid password data.",
                    BuildContext(entityId, UserChangeType.PasswordChange),
                    ex
                );
            }
        }

        private static Dictionary<string, object> BuildContext<T>(
            T entityId,
            UserChangeType changeType,
            LoginType? loginType = null)
            where T : notnull
        {
            var dict = new Dictionary<string, object>
            {
                ["Entity"] = nameof(UserPendingChange),
                ["EntityId"] = entityId,
                ["Property"] = UserChangeDataProperty,
                ["UserChangeType"] = changeType
            };

            if (loginType is not null)
                dict["LoginType"] = loginType;

            return dict;
        }
    }
}
