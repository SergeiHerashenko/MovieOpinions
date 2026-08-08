using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Authorization.Infrastructure.Persistence.Repositories.UserPendingRegistrationRepository.Ado.Mappers;
using Authorization.Infrastructure.Persistence.Repositories.UserPendingRegistrationRepository.Ado.Ordinals;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Authorization.Infrastructure.Persistence.Repositories.UserPendingRegistrationRepository.Ado
{
    internal class AdoUserPendingRegistrationQueryRepository : QueryRepositoryBase<AdoUserPendingRegistrationQueryRepository>
    {
        public AdoUserPendingRegistrationQueryRepository(
            ILogger<AdoUserPendingRegistrationQueryRepository> logger,
            IDbConnectionProvider dbConnectionProvider)
            : base(logger, dbConnectionProvider) { }

        internal async Task<UserPendingRegistration?> GetPendingUserByLoginAsync(Login login, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var sql = @"SELECT 
                                id, login, login_type, country_code, email_domain, password_hash, registration_token, expires_at, created_at 
                            FROM 
                                User_Pending_Registration 
                            WHERE 
                                login = @Login;";

                await using var command = new NpgsqlCommand(sql, connection);

                command.Parameters.Add(new NpgsqlParameter("@Login", NpgsqlDbType.Varchar) { Value = login.Value });

                await using var reader = await command.ExecuteReaderAsync(ct);

                if (!await reader.ReadAsync(ct))
                    return null;

                var ordinals = new UserPendingRegistrationOrdinals(reader);

                return UserPendingRegistrationMapper.Map(reader, ordinals);
            }, cancellationToken);
        }

        internal async Task<UserPendingRegistration?> GetPendingUserByTokenAsync(RegistrationFlowToken registrationFlowToken, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var sql = @"SELECT 
                                id, login, login_type, country_code, email_domain, password_hash, registration_token, expires_at, created_at 
                            FROM 
                                User_Pending_Registration 
                            WHERE 
                                registration_token = @RegistrationToken;";

                await using var command = new NpgsqlCommand(sql, connection);

                command.Parameters.Add(new NpgsqlParameter("@RegistrationToken", NpgsqlDbType.Varchar) { Value = registrationFlowToken.Value });

                await using var reader = await command.ExecuteReaderAsync(ct);

                if (!await reader.ReadAsync(ct))
                    return null;

                var ordinals = new UserPendingRegistrationOrdinals(reader);

                return UserPendingRegistrationMapper.Map(reader, ordinals);
            }, cancellationToken);
        }
    }
}
