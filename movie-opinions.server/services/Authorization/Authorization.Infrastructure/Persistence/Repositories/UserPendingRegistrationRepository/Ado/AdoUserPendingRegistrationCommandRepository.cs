using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;
using Authorization.Infrastructure.Persistence.Context;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Authorization.Infrastructure.Persistence.Repositories.UserPendingRegistrationRepository.Ado
{
    internal class AdoUserPendingRegistrationCommandRepository : CommandRepositoryBase<AdoUserPendingRegistrationCommandRepository>
    {
        public AdoUserPendingRegistrationCommandRepository(
            ILogger<AdoUserPendingRegistrationCommandRepository> logger,
            ITransactionContext transactionContext)
            : base(logger, transactionContext) { }

        internal async Task CreatePendingUserAsync(UserPendingRegistration entity, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"INSERT INTO 
                                User_Pending_Registration (id, login, login_type, country_code, email_domain, password_hash, registration_token, expires_at, created_at) 
                            VALUES 
                                (@Id, @Login, @LoginType, @CountryCode, @EmailDomain, @PasswordHash, @RegistrationToken, @ExpiresAt, @CreatedAt);";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                AddPendingUserParameters(command, entity);

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        internal async Task UpdatePendingUserAsync(UserPendingRegistration entity, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"UPDATE 
                                User_Pending_Registration 
                            SET 
                                password_hash = @PasswordHash, 
                                registration_token = @RegistrationToken,
                                expires_at = @ExpiresAt 
                            WHERE 
                                id = @Id;";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = entity.Id.Value });
                command.Parameters.Add(new NpgsqlParameter("@PasswordHash", NpgsqlDbType.Varchar) { Value = entity.Password.Value });
                command.Parameters.Add(new NpgsqlParameter("@RegistrationToken", NpgsqlDbType.Varchar) { Value = entity.RegistrationFlowToken.Value });
                command.Parameters.Add(new NpgsqlParameter("@ExpiresAt", NpgsqlDbType.TimestampTz) { Value = entity.ExpiresAt });

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        internal async Task DeletePendingUserAsync(UserPendingRegistrationId id, CancellationToken cancellationToken = default)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"DELETE FROM 
                                User_Pending_Registration 
                            WHERE 
                                id = @Id;";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = id.Value });

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        private static void AddPendingUserParameters(NpgsqlCommand command, UserPendingRegistration entity)
        {
            command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = entity.Id.Value });
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
            command.Parameters.Add(new NpgsqlParameter("@PasswordHash", NpgsqlDbType.Varchar) { Value = entity.Password.Value });
            command.Parameters.Add(new NpgsqlParameter("@RegistrationToken", NpgsqlDbType.Varchar) { Value = entity.RegistrationFlowToken.Value });
            command.Parameters.Add(new NpgsqlParameter("@ExpiresAt", NpgsqlDbType.TimestampTz) { Value = entity.ExpiresAt });
            command.Parameters.Add(new NpgsqlParameter("@CreatedAt", NpgsqlDbType.TimestampTz) { Value = entity.CreatedAt });
        }
    }
}