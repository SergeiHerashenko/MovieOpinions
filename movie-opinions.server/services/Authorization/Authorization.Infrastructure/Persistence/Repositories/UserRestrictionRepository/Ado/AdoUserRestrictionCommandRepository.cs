using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Infrastructure.Persistence.Context;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRestrictionRepository.Ado
{
    internal class AdoUserRestrictionCommandRepository : CommandRepositoryBase<AdoUserRestrictionCommandRepository>
    {
        public AdoUserRestrictionCommandRepository(
            ILogger<AdoUserRestrictionCommandRepository> logger,
            ITransactionContext transactionContext)
            : base(logger, transactionContext) { }

        internal async Task CreateRestrictionAsync(UserRestriction entity, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"INSERT INTO 
                                User_Restriction (id, created_at, user_id, restriction_rule, restriction_type, reason, restricted_by, is_revoked, cancellation_date) 
                            VALUES 
                                (@Id, @CreatedAt, @UserId, @RestrictionRule, @RestrictionType, @Reason, @RestrictedBy, @IsRevoked, @CancellationDate);";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                AddUserRestrictionParameters(command, entity);

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        internal async Task UpdateRestrictionAsync(UserRestriction entity, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"UPDATE 
                                User_Restriction 
                            SET 
                                reason = @Reason,
                                is_revoked = @IsRevoked,
                                cancellation_date = @CancellationDate 
                            WHERE 
                                id = @Id;";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = entity.Id.Value });
                command.Parameters.Add(new NpgsqlParameter("@Reason", NpgsqlDbType.Varchar) { Value = DbValue(entity.Reason) });
                command.Parameters.Add(new NpgsqlParameter("@IsRevoked", NpgsqlDbType.Boolean) { Value = entity.Status });
                command.Parameters.Add(new NpgsqlParameter("@CancellationDate", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.RevokedAt) });

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        private static void AddUserRestrictionParameters(NpgsqlCommand command, UserRestriction entity)
        {
            command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = entity.Id.Value });
            command.Parameters.Add(new NpgsqlParameter("@CreatedAt", NpgsqlDbType.TimestampTz) { Value = entity.CreatedAt });
            command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Uuid) { Value = entity.UserId.Value });

            string restrictionRuleJson = JsonSerializer.Serialize(entity.RestrictionRule);
            command.Parameters.Add(new NpgsqlParameter("@RestrictionRule", NpgsqlDbType.Jsonb) { Value = restrictionRuleJson });

            command.Parameters.Add(new NpgsqlParameter("@RestrictionType", NpgsqlDbType.Varchar) { Value = entity.RestrictionType.ToString() });
            command.Parameters.Add(new NpgsqlParameter("@Reason", NpgsqlDbType.Varchar) { Value = DbValue(entity.Reason) });
            command.Parameters.Add(new NpgsqlParameter("@RestrictedBy", NpgsqlDbType.Varchar) { Value = entity.ImposedBy });
            command.Parameters.Add(new NpgsqlParameter("@IsRevoked", NpgsqlDbType.Boolean) { Value = entity.Status });
            command.Parameters.Add(new NpgsqlParameter("@CancellationDate", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.RevokedAt) });
        }
    }
}
