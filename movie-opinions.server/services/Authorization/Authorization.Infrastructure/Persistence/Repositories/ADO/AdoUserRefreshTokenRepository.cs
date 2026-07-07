using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.UsersRefreshToken;
using Authorization.Domain.UsersRefreshToken.Enums;
using Authorization.Domain.UsersRefreshToken.ValueObjects;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserRefreshTokenRepository : RepositoryBase, IUserRefreshTokenRepository
    {
        private readonly IClock _clock;

        public AdoUserRefreshTokenRepository(
            IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserRefreshTokenRepository> logger,
            IClock clock)
            : base (logger, dbConnectionProvider)
        {
            _clock = clock;
        }

        public async Task<UserRefreshToken> CreateAsync(UserRefreshToken entity, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"
                            INSERT INTO 
                                user_refresh_token (id, user_id, refresh_token, device_info, ip_address, city, status, expires_at, consumed_at, revoked_at, created_at) 
                            VALUES 
                                (@Id, @UserId, @RefreshToken, @DeviceInfo, @IpAddress, @City, @Status, @ExpiresAt, @ConsumedAt, @RevokedAt, @CreatedAt) 
                            RETURNING * ;";

                await using (var insertTokenCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(insertTokenCommand, entity);

                    using (var readerInsertTokenCommand = await insertTokenCommand.ExecuteReaderAsync(ct))
                    {
                        if(await readerInsertTokenCommand.ReadAsync(ct))
                        {
                            var ords = new UserRefreshTokenOrdinals(readerInsertTokenCommand);
                            var newToken = MapReaderToToken(readerInsertTokenCommand, ords);

                            _logger.LogInformation("Token saved to main table. Guid: {id}. Creation date: {NOW}",
                                newToken.Id.Value,
                                _clock.UtcNow
                            );

                            return newToken;
                        }
                    }
                }

                throw ReturningNoDataException.NoDataReceived(
                    $"No Token data was returned {entity.Id.Value} after inserting into the database!",
                    new Dictionary<string, object>
                    {
                        ["Entity"] = nameof(UserRefreshToken),
                        ["Method"] = nameof(CreateAsync),
                        ["Id"] = entity.Id.Value,
                        ["Date"] = _clock.UtcNow
                    }
                );
            }, cancellationToken);
        }

        public Task<UserRefreshToken> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UserRefreshToken> UpdateAsync(UserRefreshToken entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UserRefreshToken> GetTokenByIdUserAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private UserRefreshToken MapReaderToToken(NpgsqlDataReader reader, in UserRefreshTokenOrdinals ords)
        {
            var id = UserRefreshTokenId.Restore(reader.GetGuid(ords.Id));
            var userId = UserId.Restore(reader.GetGuid(ords.UserId));
            var refreshToken = RefreshToken.Restore(reader.GetString(ords.RefreshToken));

            string deviceInfoJson = reader.GetString(ords.DeviceInfo);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            DeviceInfo? deviceInfo = JsonSerializer.Deserialize<DeviceInfo>(deviceInfoJson, options);

            if (deviceInfo is null)
                throw DataConsistencyException.UnknownType(
                    $"Unknown type for {nameof(DeviceInfo)}",
                    new Dictionary<string, object>
                    {
                        ["Id"] = id,
                        ["Entity"] = nameof(UserRefreshToken),
                        ["Name"] = nameof(DeviceInfo),
                        ["Value"] = $"{deviceInfo}"
                    }
                );

            var ipAddress = IpAddress.Restore(reader.GetString(ords.IpAddress));

            var city = reader.IsDBNull(ords.City) ? null : reader.GetString(ords.City);

            if (!Enum.TryParse<TokenStatus>(reader.GetString(ords.Status), out var status))
                throw DataConsistencyException.UnknownType(
                    $"Unknow type for {nameof(TokenStatus)}",
                    new Dictionary<string, object>
                    {
                        ["Id"] = id,
                        ["Entity"] = nameof(UserRefreshToken),
                        ["Name"] = nameof(TokenStatus),
                        ["Value"] = status
                    });

            var expiresAt = reader.GetFieldValue<DateTimeOffset>(ords.ExpiresAt);
            var consumedAt = reader.IsDBNull(ords.ConsumedAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ords.ConsumedAt);
            var revokedAt = reader.IsDBNull(ords.RevokedAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ords.RevokedAt);
            var createdAt = reader.GetFieldValue<DateTimeOffset>(ords.CreatedAt);

            return UserRefreshToken.Restore(id, userId, refreshToken, deviceInfo, ipAddress, city, status, expiresAt, consumedAt, createdAt, revokedAt);
        }

        private static void AddParameters(NpgsqlCommand command, UserRefreshToken entity)
        {
            command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = entity.Id.Value });
            command.Parameters.Add(new NpgsqlParameter("@UserId", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = entity.UserId.Value });
            command.Parameters.Add(new NpgsqlParameter("@RefreshToken", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = entity.RefreshToken.Value });

            string deviceInfoJson = JsonSerializer.Serialize(entity.DeviceInfo);
            command.Parameters.Add(new NpgsqlParameter("@DeviceInfo", NpgsqlTypes.NpgsqlDbType.Jsonb) { Value = deviceInfoJson });

            command.Parameters.Add(new NpgsqlParameter("@IpAddress", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = entity.IpAddress.Value });
            command.Parameters.Add(new NpgsqlParameter("@City", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = DbValue(entity.City) });
            command.Parameters.Add(new NpgsqlParameter("@Status", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = entity.Status.ToString() });
            command.Parameters.Add(new NpgsqlParameter("@ExpiresAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = entity.ExpiresAt });
            command.Parameters.Add(new NpgsqlParameter("@ConsumedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = DbValue(entity.ConsumedAt) });
            command.Parameters.Add(new NpgsqlParameter("@RevokedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = DbValue(entity.RevokedAt) });
            command.Parameters.Add(new NpgsqlParameter("@CreatedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = entity.CreatedAt });
        }

        private readonly struct UserRefreshTokenOrdinals
        {
            public int Id { get; }

            public int UserId { get; }

            public int RefreshToken { get; }

            public int DeviceInfo { get; }

            public int IpAddress { get; }

            public int City { get; }

            public int Status { get; }

            public int ExpiresAt { get; }

            public int ConsumedAt { get; }

            public int RevokedAt { get; }

            public int CreatedAt { get; }

            public UserRefreshTokenOrdinals(NpgsqlDataReader reader)
            {
                Id = reader.GetOrdinal("id");
                UserId = reader.GetOrdinal("user_id");
                RefreshToken = reader.GetOrdinal("refresh_token");
                DeviceInfo = reader.GetOrdinal("device_info");
                IpAddress = reader.GetOrdinal("ip_address");
                City = reader.GetOrdinal("city");
                Status = reader.GetOrdinal("status");
                ExpiresAt = reader.GetOrdinal("expires_at");
                ConsumedAt = reader.GetOrdinal("consumed_at");
                RevokedAt = reader.GetOrdinal("revoked_at");
                CreatedAt = reader.GetOrdinal("created_at");
            }
        }
    }
}
