using Authorization.Application.Interfaces.Persistence;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserPendingRegistrationRepository : RepositoryBase, IUserPendingRegistrationRepository
    {
        public AdoUserPendingRegistrationRepository(
            IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserPendingRegistrationRepository> logger)
            : base(logger, dbConnectionProvider) { }

        public async Task<UserPendingRegistration> CreateAsync(UserPendingRegistration entity, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                            INSERT INTO 
                                Users_Pending_Registration (id, login_user, login_type, country_code, password_hash, created_at, expires_at) 
                            VALUES
                                (@Id, @Login, @LoginType, @CountryCode, @Password, NOW(), @ExpiresAt)
                            RETURNING * ";

                await using (var insertUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(insertUserCommand, entity);

                    await using (var readerInsertUserCommand = await insertUserCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if(await readerInsertUserCommand.ReadAsync(cancellationToken))
                        {
                            var newUser = MapReaderToUser(readerInsertUserCommand);

                            _logger.LogInformation("User {Login} saved to temporary registered table. Guid {Id}. Creation date: {Now}",
                                newUser.Login.Value,
                                newUser.Id,
                                newUser.CreatedAt
                            );

                            return newUser;
                        }
                    }
                }

                throw ReturningNoDataException.NoDataReceived(
                    $"No user data was returned {entity.Login.Value} after inserting into the database!",
                    new Dictionary<string, object>
                    {
                        ["entity"] = nameof(UserPendingRegistration),
                        ["login"] = entity.Login.Value,
                        ["id"] = entity.Id,
                    }
                );
            }, cancellationToken);
        }

        public async Task<UserPendingRegistration> UpdateAsync(UserPendingRegistration entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<UserPendingRegistration> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<UserPendingRegistration?> GetByLoginAsync(Login login, CancellationToken cancellationToken = default)
        {
            return null;
        }

        private UserPendingRegistration MapReaderToUser(NpgsqlDataReader reader)
        {
            var id = UserPendingRegistrationId.Restore(reader.GetGuid(reader.GetOrdinal("id")));

            if (!Enum.TryParse<LoginType>(reader.GetString(reader.GetOrdinal("login_type")), out var loginType))
                throw DataConsistencyException.UnknownType(
                    $"Unknown type for {nameof(LoginType)}",
                    new Dictionary<string, object>
                    {
                        ["LoginType"] = reader.GetString("login_type"),
                        ["entity"] = nameof(UserPendingRegistration),
                        ["id"] = id,
                    }
                );

            Login login = loginType switch
            {
                LoginType.Email => EmailLogin.Restore(reader.GetString(reader.GetOrdinal("login_user"))),
                LoginType.Phone => RestorePhoneLogin(reader, reader.GetString(reader.GetOrdinal("login_user"))),
                _ => throw DataConsistencyException.UnknownType(
                    $"Unknown type for {nameof(LoginType)}",
                        new Dictionary<string, object>
                        {
                            ["LoginType"] = reader.GetString("login_type"),
                            ["entity"] = nameof(UserPendingRegistration),
                            ["id"] = id,
                        }
                    )
            };

            var password = Password.Restore(reader.GetString(reader.GetOrdinal("password_hash")));

            var expiresAt = reader.GetDateTime(reader.GetOrdinal("expires_at"));
            var createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));

            return UserPendingRegistration.Restore(id, login, password, createdAt, expiresAt);
        }

        private Login RestorePhoneLogin(NpgsqlDataReader reader, string fullNumber)
        {
            var countryCode = reader.GetString(reader.GetOrdinal("country_code"));

            var phoneNumber = fullNumber.StartsWith(countryCode)
                ? fullNumber.Substring(countryCode.Length)
                : fullNumber;

            return PhoneLogin.Restore(countryCode, phoneNumber);
        }

        private static void AddParameters(NpgsqlCommand command, UserPendingRegistration entity)
        {
            command.Parameters.AddWithValue("@Id", entity.Id);
            command.Parameters.AddWithValue("@Login", entity.Login.Value);
            command.Parameters.AddWithValue("@LoginType", entity.Login.Type);
            command.Parameters.AddWithValue("@CountryCode", entity.Login switch
            {
                PhoneLogin phoneLogin => DbValue(phoneLogin.Phone.CountryCode.Value),
                _ => DBNull.Value
            });
            command.Parameters.AddWithValue("@Password", entity.Password);
            command.Parameters.AddWithValue("@ExpiresAt", entity.ExpiresAt);
        }
    }
}
