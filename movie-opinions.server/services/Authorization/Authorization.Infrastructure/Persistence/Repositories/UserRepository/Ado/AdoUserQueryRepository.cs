using Authorization.Domain.Users;
using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.Entities.UsersPendingChange;
using Authorization.Domain.Users.Entities.UsersPendingChange.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers;
using Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals;
using Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado
{
    internal class AdoUserQueryRepository : QueryRepositoryBase<AdoUserQueryRepository>
    {
        public AdoUserQueryRepository(
            ILogger<AdoUserQueryRepository> logger,
            IDbConnectionProvider dbConnectionProvider)
            : base(logger, dbConnectionProvider) { }

        internal async Task<User?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var userData = await LoadUserDataByIdAsync(connection, userId, ct);

                if (userData is null)
                    return null;

                return await BuildUserAggregateAsync(connection, userData, ct);
            }, cancellationToken);
        }

        internal async Task<User?> GetUserByLoginAsync(Login login, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var userData = await LoadUserDataByLoginAsync(connection, login, ct);

                if (userData is null)
                    return null;

                return await BuildUserAggregateAsync(connection, userData, ct);
            }, cancellationToken);
        }

        internal async Task<bool> ExistsUserByLoginAsync(Login login, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var sql = @"SELECT EXISTS(SELECT 1 FROM User_Account WHERE login = @Login)";

                await using var command = new NpgsqlCommand(sql, connection);

                command.Parameters.Add(new NpgsqlParameter("@Login", NpgsqlDbType.Varchar) { Value = login.Value });

                var result = await command.ExecuteScalarAsync(ct);

                return (bool)result!;
            }, cancellationToken);
        }

        private async Task<User?> BuildUserAggregateAsync(NpgsqlConnection connection, UserData userData, CancellationToken cancellationToken = default)
        {
            var restrictionSessions = await LoadUserRestrictionSession(connection, userData.Id, cancellationToken);

            var restrictionsIds = restrictionSessions
                .SelectMany(x => x.ActiveRestrictionsIds)
                .Distinct()
                .ToList();

            var restrictions = restrictionsIds.Count == 0
                ? Array.Empty<UserRestriction>()
                : await LoadUserRestrictionAsync(connection, restrictionsIds, cancellationToken);

            var userDeletion = await LoadUserDeletionAsync(connection, userData.Id, cancellationToken);

            var refreshToken = await LoadUserRefreshTokenAsync(connection, userData.Id, cancellationToken);

            var pendingChange = await LoadUserPendingChangeAsync(connection, userData.Id, cancellationToken);

            return User.Restore(
                userData.Id,
                userData.CreatedAt,
                userData.Login,
                userData.Password,
                userData.Role,
                userData.UpdatedAt,
                userData.LastLoginAt,
                userData.IsLoginConfirmed,
                userData.FailedLoginAttempts,
                restrictions,
                restrictionSessions,
                refreshToken,
                pendingChange,
                userDeletion
            );
        }

        #region Aggregate loading
        private async Task<UserData?> LoadUserDataByLoginAsync(NpgsqlConnection connection, Login login, CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT 
                            id, created_at, login_type, login, country_code, email_domain, password_hash, role, updated_at, last_login_at, is_login_confirmed, failed_login_attempts 
                        FROM 
                            User_Account 
                        WHERE 
                            login = @Login;";

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.Add(new NpgsqlParameter("@Login", NpgsqlDbType.Varchar) { Value = login.Value });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            var ordinals = new UserOrdinals(reader);

            return UserDataMapper.Map(reader, ordinals);
        }

        private async Task<UserData?> LoadUserDataByIdAsync(NpgsqlConnection connection, UserId userId, CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT 
                            id, created_at, login_type, login, country_code, email_domain, password_hash, role, updated_at, last_login_at, is_login_confirmed, failed_login_attempts 
                        FROM 
                            User_Account 
                        WHERE 
                            id = @Id;";

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = userId.Value });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            var ordinals = new UserOrdinals(reader);

            return UserDataMapper.Map(reader, ordinals);
        }

        private async Task<IReadOnlyList<UserRestrictionSession>> LoadUserRestrictionSession(NpgsqlConnection connection, UserId userId, CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT 
                            id, created_at, user_id, active_restrictions_ids, restriction_type, total_blocked_minutes 
                        FROM 
                            User_Restriction_Session 
                        WHERE 
                            userId = @UserId;";

            await using var command = new NpgsqlCommand(sql, connection);

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

        private async Task<IReadOnlyList<UserRestriction>> LoadUserRestrictionAsync(NpgsqlConnection connection, IReadOnlyList<UserRestrictionId> restrictionsIds, CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT
                            id, created_at, user_id, restriction_rule, restriction_type, reason, restricted_by, is_revoked, cancellation_date 
                        FROM 
                            User_Restriction 
                        WHERE 
                            id = ANY(@Ids);";

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.Add(new NpgsqlParameter("@Ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid) { 
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

        private async Task<UserDeletion?> LoadUserDeletionAsync(NpgsqlConnection connection, UserId userId, CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT 
                            id, created_at, user_id, login, login_type, country_code, email_domain, reason, restore_until, restored_at, status, updated_at 
                        FROM 
                            User_Deleted 
                        WHERE 
                            user_id = @UserId;";

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Uuid) { Value = userId.Value });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            var ordinals = new UserDeletionOrdinals(reader);

            return UserDeletionMapper.Map(reader, ordinals);
        }

        private async Task<IReadOnlyList<UserRefreshToken>> LoadUserRefreshTokenAsync(NpgsqlConnection connection, UserId userId, CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT 
                            id, created_at, user_id, refresh_token, device_info, ip_address, city, token_status, expires_at, consumed_at, revoked_at 
                        FROM 
                            user_refresh_token 
                        WHERE 
                            user_id = @UserId AND token_status = @Status";

            await using var command = new NpgsqlCommand(sql, connection);

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

        private async Task<UserPendingChange?> LoadUserPendingChangeAsync(NpgsqlConnection connection, UserId userId, CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT 
                            id, created_at, user_id, confirmation_token, change_type, login_type, change_data, expires_at, confirmation_time, expired_at, status 
                        FROM 
                            User_pending_change 
                        WHERE 
                            user_id = @UserId AND status = @Status;";

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Uuid) { Value = userId.Value });
            command.Parameters.Add(new NpgsqlParameter("@Status", NpgsqlDbType.Varchar) { Value = ChangeStatus.Active.ToString() });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            var ordinals = new UserPendingChangeOrdinals(reader);

            return UserPendingChangeMapper.Map(reader, ordinals);
        }
        #endregion
    }
}
