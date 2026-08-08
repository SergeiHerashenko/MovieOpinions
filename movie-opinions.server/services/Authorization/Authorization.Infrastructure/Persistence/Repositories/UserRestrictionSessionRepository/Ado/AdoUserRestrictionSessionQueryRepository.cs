using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers;
using Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRestrictionSessionRepository.Ado
{
    internal class AdoUserRestrictionSessionQueryRepository : QueryRepositoryBase<AdoUserRestrictionSessionQueryRepository>
    {
        public AdoUserRestrictionSessionQueryRepository(
            ILogger<AdoUserRestrictionSessionQueryRepository> logger,
            IDbConnectionProvider dbConnectionProvider)
            : base(logger, dbConnectionProvider) { }

        internal async Task<UserRestrictionSession?> GetRestrictionSessionByIdAsync(UserRestrictionSessionId userRestrictionSessionId, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var sql = @"SELECT 
                            id, created_at, user_id, active_restrictions_ids, restriction_type, total_blocked_minutes 
                        FROM 
                            User_Restriction_Session 
                        WHERE 
                            id = @Id;";

                await using var command = new NpgsqlCommand(sql, connection);

                command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = userRestrictionSessionId.Value });

                await using var reader = await command.ExecuteReaderAsync(ct);

                var ordinals = new UserRestrictionSessionOrdinals(reader);

                if (await reader.ReadAsync(ct))
                {
                    return UserRestrictionSessionMapper.Map(reader, ordinals);
                }

                return null;
            }, cancellationToken);
        }

        internal async Task<UserRestrictionSession?> GetRestrictionSessionByUserIdAndTypeAsync(UserId userId, RestrictionType type, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var sql = @"SELECT 
                            id, created_at, user_id, active_restrictions_ids, restriction_type, total_blocked_minutes 
                        FROM 
                            User_Restriction_Session 
                        WHERE 
                            user_id = @UserId AND restriction_type = @RestrictionType;";

                await using var command = new NpgsqlCommand(sql, connection);

                command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Uuid) { Value = userId.Value });
                command.Parameters.Add(new NpgsqlParameter("@RestrictionType", NpgsqlDbType.Varchar) { Value = type.ToString() });

                await using var reader = await command.ExecuteReaderAsync(ct);

                var ordinals = new UserRestrictionSessionOrdinals(reader);

                if (await reader.ReadAsync(ct))
                {
                    return UserRestrictionSessionMapper.Map(reader, ordinals);
                }

                return null;
            }, cancellationToken);
        }

        internal async Task<IReadOnlyList<UserRestrictionSession>> GetRestrictionsSessionsByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var sql = @"SELECT 
                            id, created_at, user_id, active_restrictions_ids, restriction_type, total_blocked_minutes 
                        FROM 
                            User_Restriction_Session 
                        WHERE 
                            user_id = @UserId;";

                await using var command = new NpgsqlCommand(sql, connection);

                command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Uuid) { Value = userId.Value });

                await using var reader = await command.ExecuteReaderAsync(ct);

                var sessions = new List<UserRestrictionSession>();

                var ordinals = new UserRestrictionSessionOrdinals(reader);

                while (await reader.ReadAsync(ct))
                {
                    sessions.Add(UserRestrictionSessionMapper.Map(reader, ordinals));
                }

                return sessions;
            }, cancellationToken);
        }
    }
}
