using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
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
    internal static class UserPendingActionMapper
    {
        private const string UserActionDataProperty = "UserActionData";

        public static UserPendingAction Map(NpgsqlDataReader reader, UserPendingActionOrdinals ordinals)
        {
            var id = UserPendingActionId.Restore(reader.GetGuid(ordinals.Id));
            var createdAt = reader.GetFieldValue<DateTimeOffset>(ordinals.CreatedAt);

            var userId = UserId.Restore(reader.GetGuid(ordinals.UserId));

            var confirmationToken = ConfirmationFlowToken.Parse(reader.GetString(ordinals.ConfirmationToken));

            var actionType = EnumMapper.Restore<UserActionType>(
                reader.GetString(ordinals.UserActionType),
                nameof(UserPendingAction),
                id.Value
            );

            var userActionDataJson = reader.GetString(ordinals.UserActionData);

            UserAction action = actionType switch
            {
                UserActionType.ChangePassword => RestorePasswordChangeAction(userActionDataJson, id.Value),
                UserActionType.ChangeLogin => RestoreLoginChangeAction(reader, ordinals.LoginType, id.Value, userActionDataJson),
                UserActionType.DeleteUser => DeleteUserAction.Restore(DeletionReason.Restore(userActionDataJson)),

                _ => throw DataConsistencyException.UnknownType(
                    $"Unknown type for {nameof(UserActionType)}",
                    new Dictionary<string, object>
                    {
                        ["UserActionType"] = actionType,
                        ["Entity"] = nameof(UserPendingAction),
                        ["Id"] = id.Value
                    }
                )
            };

            var expiresAt = reader.GetFieldValue<DateTimeOffset>(ordinals.ExpiresAt);

            var confirmationTime = reader.IsDBNull(ordinals.ConfirmationTime) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ordinals.ConfirmationTime);
            var expiredAt = reader.IsDBNull(ordinals.ExpiredAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ordinals.ExpiredAt);
            var cancelledTime = reader.IsDBNull(ordinals.CancelledTime) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ordinals.CancelledTime);

            var status = EnumMapper.Restore<ActionStatus>(
                reader.GetString(ordinals.Status),
                nameof(UserPendingAction),
                id.Value
            );

            return UserPendingAction.Restore(id, userId, confirmationToken, action, expiresAt, confirmationTime, expiredAt, cancelledTime, status, createdAt);
        }

        private static ChangeLoginAction RestoreLoginChangeAction<T>(NpgsqlDataReader reader, int loginTypeOrd, T entityId, string json)
            where T : notnull
        {
            var loginType = EnumMapper.Restore<LoginType>(
                reader.GetString(loginTypeOrd),
                nameof(UserPendingAction),
                entityId
            );

            return loginType switch
            {
                LoginType.Email => ChangeLoginAction.Restore(RestoreEmail(json, entityId)),
                LoginType.Phone => ChangeLoginAction.Restore(RestorePhone(json, entityId)),
                _ => throw DataConsistencyException.UnknownType(
                    $"Unknown type for {nameof(LoginType)}",
                    new Dictionary<string, object>
                    {
                        ["LoginType"] = loginType,
                        ["Entity"] = nameof(UserPendingAction),
                        ["EntityId"] = entityId
                    }
                )
            };
        }

        private static ChangePasswordAction RestorePasswordChangeAction<T>(string json, T entityId)
            where T : notnull
        {
            try
            {
                var passwordHash = JsonSerializer.Deserialize<PasswordHash>(json)
                    ?? throw DataConsistencyException.InvalidData(
                        "UserActionData contains null password data.",
                        BuildContext(entityId, UserActionType.ChangePassword)
                    );

                return ChangePasswordAction.Restore(passwordHash);
            }
            catch (JsonException ex)
            {
                throw DataConsistencyException.InvalidData(
                    "UserActionData contains invalid password data.",
                    BuildContext(entityId, UserActionType.ChangePassword),
                    ex
                );
            }
        }

        private static Email RestoreEmail(string json, object entityId)
        {
            try
            {
                return JsonSerializer.Deserialize<Email>(json)
                    ?? throw DataConsistencyException.InvalidData(
                        "UserActionData contains null email data.",
                        BuildContext(entityId, UserActionType.ChangeLogin, LoginType.Email)
                    );
            }
            catch (JsonException ex)
            {
                throw DataConsistencyException.InvalidData(
                    "UserActionData contains invalid email data.",
                    BuildContext(entityId, UserActionType.ChangeLogin, LoginType.Email),
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
                        "UserActionData contains null phone data.",
                        BuildContext(entityId, UserActionType.ChangeLogin, LoginType.Phone)
                    );
            }
            catch (JsonException ex)
            {
                throw DataConsistencyException.InvalidData(
                    "UserActionData contains invalid phone data.",
                    BuildContext(entityId, UserActionType.ChangeLogin, LoginType.Phone),
                    ex
                );
            }
        }

        private static Dictionary<string, object> BuildContext<T>(
            T entityId,
            UserActionType actionType,
            LoginType? loginType = null)
            where T : notnull
        {
            var dict = new Dictionary<string, object>
            {
                ["Entity"] = nameof(UserPendingAction),
                ["EntityId"] = entityId,
                ["Property"] = UserActionDataProperty,
                ["UserActionType"] = actionType
            };

            if (loginType is not null)
                dict["LoginType"] = loginType;

            return dict;
        }
    }
}
