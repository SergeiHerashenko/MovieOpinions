using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Infrastructure.Persistence.Context;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRestrictionSessionRepository.Ado
{
    internal class AdoUserRestrictionSessionCommandRepository : CommandRepositoryBase<AdoUserRestrictionSessionCommandRepository>
    {
        public AdoUserRestrictionSessionCommandRepository(
            ILogger<AdoUserRestrictionSessionCommandRepository> logger,
            ITransactionContext transactionContext)
            : base(logger, transactionContext) { }

        internal async Task CreateRestrictionSessionAsync(UserRestrictionSession entity, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"INSERT INTO 
                                User_Restriction_Session (id, created_at, user_id, active_restrictions_ids, restriction_type, total_blocked_minutes) 
                            VALUES 
                                (@Id, @CreatedAt, @UserId, @ActiveRestrictionsIds, @RestrictionType, @TotalBlockedMinutes);";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                AddUserRestrictionSessionParameters(command, entity);

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        internal async Task UpdateRestrictionSessionAsync(UserRestrictionSession entity, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"UPDATE 
                                User_Restriction_Session 
                            SET 
                                active_restrictions_ids = @ActiveRestrictionsIds,
                                total_blocked_minutes = @TotalBlockedMinutes
                            WHERE 
                                id = @Id;";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = entity.Id.Value });
                command.Parameters.Add(new NpgsqlParameter("@ActiveRestrictionsIds", NpgsqlDbType.Jsonb)
                {
                    Value = JsonSerializer.Serialize(entity.ActiveRestrictionIds.Select(x => x.Value))
                });
                command.Parameters.Add(new NpgsqlParameter("@TotalBlockedMinutes", NpgsqlDbType.Integer) { Value = entity.TotalBlockedMinutes });

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        internal async Task DeleteRestrictionSessionAsync(UserRestrictionSessionId userRestrictionSessionId, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"DELETE FROM 
                                User_Restriction_Session 
                            WHERE 
                                id = @Id;";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = userRestrictionSessionId.Value });

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        private static void AddUserRestrictionSessionParameters(NpgsqlCommand command, UserRestrictionSession entity)
        {
            command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = entity.Id.Value });
            command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Uuid) { Value = entity.UserId.Value });
            command.Parameters.Add(new NpgsqlParameter("@ActiveRestrictionsIds", NpgsqlDbType.Jsonb)
            {
                Value = JsonSerializer.Serialize(entity.ActiveRestrictionIds.Select(x => x.Value))
            });
            command.Parameters.Add(new NpgsqlParameter("@RestrictionType", NpgsqlDbType.Varchar) { Value = entity.RestrictionType.ToString() });
            command.Parameters.Add(new NpgsqlParameter("@TotalBlockedMinutes", NpgsqlDbType.Integer) { Value = entity.TotalBlockedMinutes });
            command.Parameters.Add(new NpgsqlParameter("@CreatedAt", NpgsqlDbType.TimestampTz) { Value = entity.CreatedAt });
        }
    }
}
