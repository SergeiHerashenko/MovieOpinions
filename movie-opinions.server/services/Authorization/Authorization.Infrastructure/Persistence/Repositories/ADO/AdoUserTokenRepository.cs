using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Text.Json;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserTokenRepository : RepositoryBase, IUserRefreshTokenRepository
    {
        public AdoUserTokenRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserTokenRepository> logger)
                : base(logger, dbConnectionProvider) { }

        public async Task<UserRefreshToken> CreateAsync(UserRefreshToken entity, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        INSERT INTO 
                            User_Tokens (token_id, user_id, refresh_token, device_info, ip_address, city, expiration_token, is_used, is_revoked, created_at) 
                        VALUES
                            (@Id, @UserId, @RefreshToken, @DeviceInfo, @IpAddress, @City, @ExpirationToken, @IsUsed, @IsRevoked, NOW())
                        RETURNING * ";

                await using (var createdTokenCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(createdTokenCommand, entity);

                    await using (var readerCreatedTokenCommand = await createdTokenCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerCreatedTokenCommand.ReadAsync(cancellationToken))
                        {
                            var newToken = MapReaderToToken(readerCreatedTokenCommand);

                            _logger.LogInformation("Токен створено для користувача {UserId}. Guid {Id}. Дата створення {Now}",
                                newToken.UserId,
                                newToken.Id,
                                DateTime.UtcNow);

                            return newToken;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserPendingChange), entity.Id);
            }, cancellationToken);
        }

        public async Task<UserRefreshToken> UpdateAsync(UserRefreshToken entity, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        UPDATE 
                            User_Tokens 
                        SET 
                            user_id = @UserId, 
                            refresh_token = @RefreshToken,
                            device_info = @DeviceInfo, 
                            ip_address = @IpAddress, 
                            city = @City, 
                            expiration_token = @ExpirationToken, 
                            is_used = @IsUsed, 
                            is_revoked = @IsRevoked 
                        WHERE 
                            token_id = @Id 
                        RETURNING * ";

                await using (var updateTokenCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(updateTokenCommand, entity);

                    await using (var readerUpdateTokenCommand = await updateTokenCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerUpdateTokenCommand.ReadAsync(cancellationToken))
                        {
                            var updateToken = MapReaderToToken(readerUpdateTokenCommand);

                            _logger.LogInformation("Токен успішно оновлений для користувача {UserId}. Guid {Id}. Дата оновлення {Now}",
                                updateToken.UserId,
                                updateToken.Id,
                                DateTime.UtcNow);

                            return updateToken;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserPendingChange), entity.Id);
            }, cancellationToken);
        }

        public async Task<UserRefreshToken> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        DELETE FROM 
                            User_Tokens 
                        WHERE
                            token_id = @Id
                        RETURNING * ";

                await using (var deleteTokenCommand = new NpgsqlCommand(sql, conn))
                {
                    deleteTokenCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerDeleteTokenCommand = await deleteTokenCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerDeleteTokenCommand.ReadAsync(cancellationToken))
                        {
                            var deleteToken = MapReaderToToken(readerDeleteTokenCommand);

                            _logger.LogInformation("Токен користувача {UserId} успішно видалений. Guid {Id}. Дата видалення {Now}",
                                deleteToken.UserId,
                                deleteToken.Id,
                                DateTime.UtcNow);

                            return deleteToken;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserPendingChange), id);
            }, cancellationToken);
        }

        public async Task<IEnumerable<UserRefreshToken>> GetAllTokensUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var userTokensList = new List<UserRefreshToken>();

            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                    SELECT 
                        token_id, user_id, refresh_token, device_info, ip_address, city, expiration_token, is_used, is_revoked, created_at 
                    FROM
                        User_Tokens
                    WHERE
                        user_id = @Id ";

                await using (var getTokenCommand = new NpgsqlCommand(sql, conn))
                {
                    getTokenCommand.Parameters.AddWithValue("@Id", userId);

                    await using (var readerGetTokenCommand = await getTokenCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await readerGetTokenCommand.ReadAsync(cancellationToken))
                        {
                            var tokenEntity = MapReaderToToken(readerGetTokenCommand);

                            userTokensList.Add(tokenEntity);
                        }

                        return userTokensList;
                    }
                }
            }, cancellationToken);
        }

        public async Task<UserRefreshToken?> GetUserTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                    SELECT 
                        token_id, user_id, refresh_token, device_info, ip_address, city, expiration_token, is_used, is_revoked, created_at 
                    FROM
                        User_Tokens
                    WHERE
                        refresh_token = @RefreshToken ";

                await using (var getTokenCommand = new NpgsqlCommand(sql, conn))
                {
                    getTokenCommand.Parameters.AddWithValue("@RefreshToken", refreshToken);

                    await using (var readerGetTokenCommand = await getTokenCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerGetTokenCommand.ReadAsync(cancellationToken))
                        {
                            var tokenEntity = MapReaderToToken(readerGetTokenCommand);

                            _logger.LogInformation("Токен користувача {UserId} знайдений!", tokenEntity.UserId);

                            return tokenEntity;
                        }
                    }
                }

                return null;
            }, cancellationToken);
        }

        private static void AddParameters(NpgsqlCommand cmd, UserRefreshToken entity)
        {
            cmd.Parameters.AddWithValue("@Id", entity.Id);
            cmd.Parameters.AddWithValue("@UserId", entity.UserId);
            cmd.Parameters.AddWithValue("@RefreshToken", entity.RefreshToken);

            var deviceInfoJson = JsonSerializer.Serialize(entity.DeviceInfo);
            cmd.Parameters.AddWithValue("@DeviceInfo", deviceInfoJson);

            cmd.Parameters.AddWithValue("@IpAddress", entity.IpAddress);
            cmd.Parameters.AddWithValue("@City", DbValue(entity.City));
            cmd.Parameters.AddWithValue("@ExpirationToken", entity.ExpiresAt);

            cmd.Parameters.AddWithValue("@IsUsed", entity.IsUsed);
            cmd.Parameters.AddWithValue("@IsRevoked", entity.IsRevoked);
        }

        private UserRefreshToken MapReaderToToken(NpgsqlDataReader reader)
        {
            var id = reader.GetGuid("token_id");
            var userId = reader.GetGuid("user_id");
            var refreshToken = reader.GetString("refresh_token");

            var deviceInfo = JsonSerializer.Deserialize<DeviceInfo>(reader.GetString("device_info"))
                ?? throw new DataConsistencyException("Failed to deserialize DeviceInfo");

            var ipAddress = reader.GetString("ip_address");

            var city = reader.IsDBNull("city") ? null : reader.GetString("city");

            var expiresAt = reader.GetDateTime("expiration_token");

            var isUsed = reader.GetBoolean("is_used");
            var isRevoked = reader.GetBoolean("is_revoked");
            var createdAt = reader.GetDateTime("created_at");

            return UserRefreshToken.Restore(id, userId, refreshToken, deviceInfo, ipAddress, city, expiresAt, createdAt, isUsed, isRevoked);
        }
    }
}
