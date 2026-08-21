using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Infrastructure.Persistence.Context;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Authorization.Infrastructure.Persistence.Repositories.UserDeletedRepository.Ado
{
    internal class AdoUserDeletedCommandRepository : CommandRepositoryBase<AdoUserDeletedCommandRepository>
    {
        public AdoUserDeletedCommandRepository(
            ILogger<AdoUserDeletedCommandRepository> logger,
            ITransactionContext transactionContext)
            : base(logger, transactionContext) { }

        internal async Task CreateDeletedUserAsync(UserDeletion entity, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"INSERT INTO 
                                User_Deleted (id, user_id, login, login_type, country_code, email_domain, reason, restore_until, restored_at, status, updated_at, created_at) 
                            VALUES 
                                (@Id, @UserId, @Login, @LoginType, @CountryCode, @EmailDomain, @Reason, @RestoreUntil, @RestoredAt, @Status, @UpdateAt, @CreatedAt);";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                AddDeletedUserParameters(command, entity);

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        internal async Task UpdateDeletedUserAsync(UserDeletion entity, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"UPDATE 
                                User_Deleted 
                            SET 
                                status = @Status,
                                restored_at = @RestoredAt,
                                updated_at = @UpdateAt 
                            WHERE 
                                id = @Id;";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = entity.Id.Value });
                command.Parameters.Add(new NpgsqlParameter("@RestoredAt", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.RestoredAt) });
                command.Parameters.Add(new NpgsqlParameter("@Status", NpgsqlDbType.Varchar) { Value = entity.Status.ToString() });
                command.Parameters.Add(new NpgsqlParameter("@UpdateAt", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.UpdatedAt) });

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        private static void AddDeletedUserParameters(NpgsqlCommand command, UserDeletion entity)
        {
            command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = entity.Id.Value });
            command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Uuid) { Value = entity.UserId.Value });
            command.Parameters.Add(new NpgsqlParameter("@Login", NpgsqlDbType.Varchar) { Value = entity.Login.Value });
            command.Parameters.Add(new NpgsqlParameter("@LoginType", NpgsqlDbType.Varchar) { Value = entity.Login.Type.ToString() });
            command.Parameters.Add(new NpgsqlParameter("@CountryCode", NpgsqlDbType.Varchar) 
            { 
                Value = entity.Login switch
                {
                    PhoneLogin phoneLogin => phoneLogin.Phone.PhoneCountryCode.Value,
                    _ => DBNull.Value
                } 
            });
            command.Parameters.Add(new NpgsqlParameter("@EmailDomain", NpgsqlDbType.Varchar)
            {
                Value = entity.Login switch
                {
                    EmailLogin emailLogin => emailLogin.Email.EmailDomainPart.Value,
                    _ => DBNull.Value
                }
            });
            command.Parameters.Add(new NpgsqlParameter("@Reason", NpgsqlDbType.Varchar) { Value = DbValue(entity.Reason.Value) });
            command.Parameters.Add(new NpgsqlParameter("@RestoreUntil", NpgsqlDbType.TimestampTz) { Value = entity.RestoreUntil });
            command.Parameters.Add(new NpgsqlParameter("@RestoredAt", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.RestoredAt) });
            command.Parameters.Add(new NpgsqlParameter("@Status", NpgsqlDbType.Varchar) { Value = entity.Status.ToString() });
            command.Parameters.Add(new NpgsqlParameter("@UpdateAt", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.UpdatedAt) });
            command.Parameters.Add(new NpgsqlParameter("@CreatedAt", NpgsqlDbType.TimestampTz) { Value = entity.CreatedAt });
        }
    }
}
