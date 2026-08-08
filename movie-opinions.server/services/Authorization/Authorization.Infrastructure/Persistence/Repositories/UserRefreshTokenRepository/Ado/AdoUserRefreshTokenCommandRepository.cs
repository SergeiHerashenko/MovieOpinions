using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Infrastructure.Persistence.Context;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRefreshTokenRepository.Ado
{
    internal class AdoUserRefreshTokenCommandRepository : CommandRepositoryBase<AdoUserRefreshTokenCommandRepository>
    {
        public AdoUserRefreshTokenCommandRepository(
            ILogger<AdoUserRefreshTokenCommandRepository> logger,
            ITransactionContext transactionContext)
            : base(logger, transactionContext) { }

        internal async Task CreateRefreshTokenAsync(UserRefreshToken userRefreshToken, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"INSERT INTO 
                                User_Refresh_Token (id, user_id, refresh_token, device_info, ip_address, city, token_status, expires_at, consumed_at, revoked_at, created_at) 
                            VALUES 
                                (@Id, @UserId, @RefreshToken, @DeviceInfo, @IpAddress, @City, @TokenStatus, @ExpiresAt, @ConsumedAt, @RevokedAt, @CreatedAt);";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                AddUserRefreshTokenParameters(command, userRefreshToken);
                
                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        internal async Task UpdateRefreshTokenAsync(UserRefreshToken userRefreshToken, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"UPDATE 
                                User_Refresh_Token 
                            SET 
                                token_status = @TokenStatus,
                                consumed_at = @ConsumedAt,
                                revoked_at = @RevokedAt
                            WHERE
                                id = @Id;";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                command.Parameters.Add(new NpgsqlParameter("@TokenStatus", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = userRefreshToken.TokenStatus.ToString() });
                command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = userRefreshToken.Id.Value });
                command.Parameters.Add(new NpgsqlParameter("@ConsumedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = userRefreshToken.ConsumedAt });
                command.Parameters.Add(new NpgsqlParameter("@RevokedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = userRefreshToken.RevokedAt });

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        private static void AddUserRefreshTokenParameters(NpgsqlCommand command, UserRefreshToken entity)
        {
            command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = entity.Id.Value });
            command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = entity.UserId.Value });
            command.Parameters.Add(new NpgsqlParameter("@RefreshToken", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = entity.RefreshToken.Value });

            string deviceInfoJson = JsonSerializer.Serialize(entity.DeviceInfo);
            command.Parameters.Add(new NpgsqlParameter("@DeviceInfo", NpgsqlTypes.NpgsqlDbType.Jsonb) { Value = deviceInfoJson });

            command.Parameters.Add(new NpgsqlParameter("@IpAddress", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = entity.IpAddress.Value });
            command.Parameters.Add(new NpgsqlParameter("@City", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = DbValue(entity.City) });
            command.Parameters.Add(new NpgsqlParameter("@TokenStatus", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = entity.TokenStatus.ToString() });
            command.Parameters.Add(new NpgsqlParameter("@ExpiresAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = entity.ExpiresAt });
            command.Parameters.Add(new NpgsqlParameter("@ConsumedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = DbValue(entity.ConsumedAt) });
            command.Parameters.Add(new NpgsqlParameter("@RevokedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = DbValue(entity.RevokedAt) });
            command.Parameters.Add(new NpgsqlParameter("@CreatedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = entity.CreatedAt });
        }
    }
}
