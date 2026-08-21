using Authorization.Domain.Users;
using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers;
using Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals;
using Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado.Models;
using Npgsql;
using NpgsqlTypes;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado.Loading
{
    internal sealed class AdoUserAggregateLoader
    {
        internal async Task<User?> BuildUserAggregateAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction? transaction,
            UserData userData, 
            CancellationToken cancellationToken = default)
        {
            var restrictionSessions = await LoadUserRestrictionSession(connection, transaction, userData.Id, cancellationToken);

            var restrictionsIds = restrictionSessions
                .SelectMany(x => x.ActiveRestrictionsIds)
                .Distinct()
                .ToList();

            var restrictions = restrictionsIds.Count == 0
                ? Array.Empty<UserRestriction>()
                : await LoadUserRestrictionAsync(connection, transaction, restrictionsIds, cancellationToken);

            var userDeletion = await LoadUserDeletionAsync(connection, transaction, userData.Id, cancellationToken);

            var refreshToken = await LoadUserRefreshTokenAsync(connection, transaction, userData.Id, cancellationToken);

            var pendingAction = await LoadUserPendingActionAsync(connection, transaction, userData.Id, cancellationToken);

            return User.Restore(
                userData.Id,
                userData.CreatedAt,
                userData.Login,
                userData.Password,
                userData.Role,
                userData.UpdatedAt,
                userData.LastLoginAt,
                userData.IsLoginConfirmed,
                userData.FailedPasswordAttempts,
                restrictions,
                restrictionSessions,
                refreshToken,
                pendingAction,
                userDeletion
            );
        }

        private async Task<IReadOnlyList<UserRestrictionSession>> LoadUserRestrictionSession(
            NpgsqlConnection connection,
            NpgsqlTransaction? transaction, 
            UserId userId, 
            CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT 
                            id, created_at, user_id, active_restrictions_ids, restriction_type, total_blocked_minutes 
                        FROM 
                            User_Restriction_Session 
                        WHERE 
                            user_id = @UserId;";

            await using var command = new NpgsqlCommand(sql, connection, transaction);

            command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Uuid) { Value = userId.Value });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var sessions = new List<UserRestrictionSession>();

            var ordinals = new UserRestrictionSessionOrdinals(reader);

            while (await reader.ReadAsync(cancellationToken))
            {
                sessions.Add(UserRestrictionSessionMapper.Map(reader, ordinals));
            }

            return sessions;
        }

        private async Task<IReadOnlyList<UserRestriction>> LoadUserRestrictionAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction? transaction, 
            IReadOnlyList<UserRestrictionId> restrictionsIds, 
            CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT
                            id, created_at, user_id, restriction_rule, restriction_type, reason, restricted_by, is_revoked, cancellation_date 
                        FROM 
                            User_Restriction 
                        WHERE 
                            id = ANY(@Ids);";

            await using var command = new NpgsqlCommand(sql, connection, transaction);

            command.Parameters.Add(new NpgsqlParameter("@Ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid)
            {
                Value = restrictionsIds.Select(x => x.Value).ToArray()
            });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var restrictions = new List<UserRestriction>();

            var ordinals = new UserRestrictionOrdinals(reader);

            while (await reader.ReadAsync(cancellationToken))
            {
                restrictions.Add(UserRestrictionMapper.Map(reader, ordinals));
            }

            return restrictions;
        }

        private async Task<UserDeletion?> LoadUserDeletionAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction? transaction,
            UserId userId, 
            CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT 
                            id, created_at, user_id, login, login_type, country_code, email_domain, reason, restore_until, restored_at, status, updated_at 
                        FROM 
                            User_Deleted 
                        WHERE 
                            user_id = @UserId;";

            await using var command = new NpgsqlCommand(sql, connection, transaction);

            command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Uuid) { Value = userId.Value });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            var ordinals = new UserDeletionOrdinals(reader);

            return UserDeletionMapper.Map(reader, ordinals);
        }

        private async Task<IReadOnlyList<UserRefreshToken>> LoadUserRefreshTokenAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction? transaction, 
            UserId userId, 
            CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT 
                            id, created_at, user_id, refresh_token, device_info, ip_address, city, token_status, expires_at, consumed_at, revoked_at 
                        FROM 
                            User_Refresh_Token 
                        WHERE 
                            user_id = @UserId AND token_status = @Status";

            await using var command = new NpgsqlCommand(sql, connection, transaction);

            command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Uuid) { Value = userId.Value });
            command.Parameters.Add(new NpgsqlParameter("@Status", NpgsqlDbType.Varchar) { Value = TokenStatus.Active.ToString() });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var refreshToken = new List<UserRefreshToken>();

            var ordinals = new UserRefreshTokenOrdinals(reader);

            while (await reader.ReadAsync(cancellationToken))
            {
                refreshToken.Add(UserRefreshTokenMapper.Map(reader, ordinals));
            }

            return refreshToken;
        }

        private async Task<UserPendingAction?> LoadUserPendingActionAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction? transaction, 
            UserId userId, 
            CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT 
                            id, created_at, user_id, confirmation_token, action_type, login_type, action_data, expires_at, confirmation_time, expired_at, cancelled_time, status 
                        FROM 
                            User_Pending_Action 
                        WHERE 
                            user_id = @UserId AND status = @Status;";

            await using var command = new NpgsqlCommand(sql, connection, transaction);

            command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Uuid) { Value = userId.Value });
            command.Parameters.Add(new NpgsqlParameter("@Status", NpgsqlDbType.Varchar) { Value = ActionStatus.Active.ToString() });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            var ordinals = new UserPendingActionOrdinals(reader);

            return UserPendingActionMapper.Map(reader, ordinals);
        }
    }
}
