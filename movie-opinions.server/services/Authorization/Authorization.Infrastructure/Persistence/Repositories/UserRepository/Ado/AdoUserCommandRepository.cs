using Authorization.Domain.Users;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Infrastructure.Persistence.Context;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado
{
    internal class AdoUserCommandRepository : CommandRepositoryBase<AdoUserCommandRepository>
    {
        public AdoUserCommandRepository(
            ILogger<AdoUserCommandRepository> logger,
            ITransactionContext transactionContext)
            : base(logger, transactionContext) { }

        internal async Task CreateUserAsync(User entity, CancellationToken cancellationToken)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"INSERT INTO 
                                User_Account (id, created_at, login_type, login, country_code, email_domain, password_hash, role, updated_at, last_login_at, is_login_confirmed, failed_login_attempts) 
                            VALUES 
                                (@Id, @CreatedAt, @LoginType, @Login, @CountryCode, @EmailDomain, @PasswordHash, @Role, @UpdatedAt, @LastLoginAt, @IsLoginConfirmed, @FailedLoginAttempts);";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                AddUserParameters(command, entity);
                command.Parameters.Add(new NpgsqlParameter("@CreatedAt", NpgsqlDbType.TimestampTz) { Value = entity.CreatedAt });

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        internal async Task UpdateUserAsync(User entity, CancellationToken cancellationToken)
        {
            await ExecuteCommandAsync(async (connection, transaction, ct) =>
            {
                var sql = @"UPDATE 
                                User_Account 
                            SET 
                                login = @Login,
                                login_type = @LoginType,
                                country_code = @CountryCode,
                                email_domain = @EmailDomain,
                                password_hash = @PasswordHash,
                                role = @Role,
                                updated_at = @UpdatedAt,
                                last_login_at = @LastLoginAt,
                                is_login_confirmed = @IsLoginConfirmed,
                                failed_login_attempts = @FailedLoginAttempts 
                            WHERE
                                id = @Id;";

                await using var command = new NpgsqlCommand(sql, connection, transaction);

                AddUserParameters(command, entity);

                await command.ExecuteNonQueryAsync(ct);
            }, cancellationToken);
        }

        private static void AddUserParameters(NpgsqlCommand command, User entity)
        {
            command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = entity.Id.Value });
            command.Parameters.Add(new NpgsqlParameter("@LoginType", NpgsqlDbType.Varchar) { Value = entity.Login.Type.ToString() });
            command.Parameters.Add(new NpgsqlParameter("@Login", NpgsqlDbType.Varchar) { Value = entity.Login.Value });
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
            command.Parameters.Add(new NpgsqlParameter("@Role", NpgsqlDbType.Varchar) { Value = entity.Role.ToString() });
            command.Parameters.Add(new NpgsqlParameter("@UpdatedAt", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.UpdatedAt) });
            command.Parameters.Add(new NpgsqlParameter("@LastLoginAt", NpgsqlDbType.TimestampTz) { Value = DbValue(entity.LastLoginAt) });
            command.Parameters.Add(new NpgsqlParameter("@IsLoginConfirmed", NpgsqlDbType.Boolean) { Value = entity.IsLoginConfirmed });
            command.Parameters.Add(new NpgsqlParameter("@FailedLoginAttempts", NpgsqlDbType.Integer) { Value = entity.FailedLoginAttempts });
        }
    }
}
