using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers;
using Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRefreshTokenRepository.Ado
{
    internal class AdoUserRefreshTokenQueryRepository : QueryRepositoryBase<AdoUserRefreshTokenQueryRepository>
    {
        public AdoUserRefreshTokenQueryRepository(
            ILogger<AdoUserRefreshTokenQueryRepository> logger,
            IDbConnectionProvider dbConnectionProvider)
            : base(logger, dbConnectionProvider) { }

        internal async Task<UserRefreshToken?> GetRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var sql = @"SELECT 
                                id, created_at, user_id, refresh_token, device_info, ip_address, city, token_status, expires_at, consumed_at, revoked_at 
                            FROM 
                                user_refresh_token 
                            WHERE 
                                refresh_token = @RefreshToken;";

                await using var command = new NpgsqlCommand(sql, connection);

                command.Parameters.Add(new NpgsqlParameter("@RefreshToken", NpgsqlDbType.Varchar) { Value = refreshToken.Value });

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                var ordinals = new UserRefreshTokenOrdinals(reader);

                if (await reader.ReadAsync(ct))
                {
                    return UserRefreshTokenMapper.Map(reader, ordinals);
                }

                return null;
            }, cancellationToken);
        }
    }
}
