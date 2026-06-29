using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserPendingRegistrationRepository : RepositoryBase, IUserPendingRegistrationRepository
    {
        private readonly IClock _clock;

        public AdoUserPendingRegistrationRepository(
            IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserPendingRegistrationRepository> logger,
            IClock clock)
            : base(logger, dbConnectionProvider)
        {
            _clock = clock;
        }

        public async Task<UserPendingRegistration> CreateAsync(UserPendingRegistration entity, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"
                            INSERT INTO 
                                User_Pending_Registration (id, login_user, login_type, country_code, password_hash, created_at, expires_at) 
                            VALUES
                                (@Id, @LoginUser, @LoginType, @CountryCode, @Password, @CreatedAt, @ExpiresAt)
                            RETURNING * ;";

                await using (var insertUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(insertUserCommand, entity);

                    await using (var readerInsertUserCommand = await insertUserCommand.ExecuteReaderAsync(ct))
                    {
                        if(await readerInsertUserCommand.ReadAsync(ct))
                        {
                            var ords = new UserPendingRegistrationOrdinals(readerInsertUserCommand);
                            var newUser = MapReaderToUser(readerInsertUserCommand, ords);

                            _logger.LogInformation("User {Login} saved to temporary registered table. Guid {Id}. Creation date: {Now}",
                                newUser.Login.Value,
                                newUser.Id.Value,
                                _clock.UtcNow
                            );

                            return newUser;
                        }
                    }
                }

                throw ReturningNoDataException.NoDataReceived(
                    $"No user data was returned {entity.Login.Value} after inserting into the database!",
                    new Dictionary<string, object>
                    {
                        ["Entity"] = nameof(UserPendingRegistration),
                        ["Method"] = nameof(CreateAsync),
                        ["Login"] = entity.Login.Value,
                        ["Id"] = entity.Id.Value,
                        ["Date"] = _clock.UtcNow
                    }
                );
            }, cancellationToken);
        }

        public async Task<UserPendingRegistration> UpdateAsync(UserPendingRegistration entity, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"
                            UPDATE 
                                User_Pending_Registration 
                            SET  
                                password_hash = @Password,
                                expires_at = @ExpiresAt 
                            WHERE 
                                login_user = @LoginUser 
                            RETURNING *;";

                await using (var updateUserCommand = new NpgsqlCommand(sql, conn))
                {
                    updateUserCommand.Parameters.Add(new NpgsqlParameter("@LoginUser", NpgsqlTypes.NpgsqlDbType.Varchar){ Value = entity.Login.Value });
                    updateUserCommand.Parameters.Add(new NpgsqlParameter("@Password", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = entity.Password.HashPassword });
                    updateUserCommand.Parameters.Add(new NpgsqlParameter("@ExpiresAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = entity.ExpiresAt });

                    await using (var readerUpdateUserCommand = await updateUserCommand.ExecuteReaderAsync(ct))
                    {
                        if(await readerUpdateUserCommand.ReadAsync(ct))
                        {
                            var ords = new UserPendingRegistrationOrdinals(readerUpdateUserCommand);
                            var updateUser = MapReaderToUser(readerUpdateUserCommand, ords);

                            _logger.LogInformation("User {Login} data successfully updated. Guid {Id}. Update date: {Now}",
                                entity.Login.Value,
                                entity.Id.Value,
                                _clock.UtcNow
                            );

                            return updateUser;
                        }
                    }
                }

                throw ReturningNoDataException.NoDataReceived(
                    $"No data was returned after updating user {entity.Login.Value}!",
                    new Dictionary<string, object>
                    {
                        ["Entity"] = nameof(UserPendingRegistration),
                        ["Method"] = nameof(UpdateAsync),
                        ["Login"] = entity.Login.Value,
                        ["Id"] = entity.Id.Value,
                        ["Date"] = _clock.UtcNow
                    }
                );
            }, cancellationToken);
        }

        public async Task<UserPendingRegistration> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"
                            DELETE FROM 
                                User_Pending_Registration 
                            WHERE 
                                id = @Id 
                            RETURNING *;";

                await using (var deleteUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deleteUserCommand.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = id });

                    await using (var readerDeletedUserCommand = await deleteUserCommand.ExecuteReaderAsync(ct))
                    {
                        if(await readerDeletedUserCommand.ReadAsync(ct))
                        {
                            var ords = new UserPendingRegistrationOrdinals(readerDeletedUserCommand);
                            var userEntity = MapReaderToUser(readerDeletedUserCommand, ords);

                            _logger.LogInformation("User {Login} has been successfully deleted from the registration table. Guid {Id}. Deleted on: {Now}",
                                userEntity.Login.Value,
                                userEntity.Id.Value,
                                _clock.UtcNow
                            );

                            return userEntity;
                        }
                    }
                }

                throw ReturningNoDataException.NoDataReceived(
                    $"No data was received for the deleted user {id}!",
                    new Dictionary<string, object>
                    {
                        ["Entity"] = nameof(UserPendingRegistration),
                        ["Method"] = nameof(DeleteAsync),
                        ["Id"] = id,
                        ["Date"] = _clock.UtcNow
                    }
                );
            }, cancellationToken);
        }

        public async Task<UserPendingRegistration?> GetByLoginAsync(Login login, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"
                            SELECT 
                                id, login_user, login_type, country_code, password_hash, created_at, expires_at 
                            FROM 
                                User_Pending_Registration 
                            WHERE 
                                login_user = @LoginUser;";

                await using (var getUserByLoginCommand = new NpgsqlCommand(sql, conn))
                {
                    getUserByLoginCommand.Parameters.Add(new NpgsqlParameter("@LoginUser", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = login.Value });

                    await using (var readerGetUserByLoginCommand = await getUserByLoginCommand.ExecuteReaderAsync(ct))
                    {
                        if(await readerGetUserByLoginCommand.ReadAsync(ct))
                        {
                            var ords = new UserPendingRegistrationOrdinals(readerGetUserByLoginCommand);
                            var userEntity = MapReaderToUser(readerGetUserByLoginCommand, ords);

                            return userEntity;
                        }
                    }
                }

                return null;
            }, cancellationToken);
        }

        private UserPendingRegistration MapReaderToUser(NpgsqlDataReader reader, in UserPendingRegistrationOrdinals ords)
        {
            var id = UserPendingRegistrationId.Restore(reader.GetGuid(ords.Id));

            if (!Enum.TryParse<LoginType>(reader.GetString(ords.LoginType), out var loginType))
                throw DataConsistencyException.UnknownType(
                    $"Unknown type for {nameof(LoginType)}",
                    new Dictionary<string, object>
                    {
                        ["LoginType"] = reader.GetString(ords.LoginType),
                        ["Entity"] = nameof(UserPendingRegistration),
                        ["Id"] = id,
                    }
                );

            Login login = loginType switch
            {
                LoginType.Email => EmailLogin.Restore(reader.GetString(ords.LoginUser)),
                LoginType.Phone => RestorePhoneLogin(reader, ords.CountryCode, reader.GetString(ords.LoginUser)),
                _ => throw DataConsistencyException.UnknownType(
                        $"Unknown type for {nameof(LoginType)}",
                        new Dictionary<string, object>
                        {
                            ["LoginType"] = reader.GetString(ords.LoginType),
                            ["Entity"] = nameof(UserPendingRegistration),
                            ["Id"] = id,
                        }
                    )
            };

            var password = Password.Restore(reader.GetString(ords.PasswordHash));

            var expiresAt = reader.GetFieldValue<DateTimeOffset>(ords.ExpiresAt);
            var createdAt = reader.GetFieldValue<DateTimeOffset>(ords.CreatedAt);

            return UserPendingRegistration.Restore(id, login, password, createdAt, expiresAt);
        }

        private Login RestorePhoneLogin(NpgsqlDataReader reader, int countryOrd, string fullNumber)
        {
            var countryCode = reader.IsDBNull(countryOrd) ? null : reader.GetString(countryOrd);

            if (countryCode is null)
                throw DataConsistencyException.UnknownType(
                    $"Unknown type for {nameof(countryCode)}",
                    new Dictionary<string, object>
                    {
                        ["ValueName"] = nameof(countryCode),
                        ["Entity"] = nameof(UserPendingRegistration),
                        ["FullNumber"] = fullNumber
                    }
                );

            var phoneNumber = fullNumber.StartsWith(countryCode)
                ? fullNumber.Substring(countryCode.Length)
                : fullNumber;

            return PhoneLogin.Restore(countryCode, phoneNumber);
        }

        private static void AddParameters(NpgsqlCommand command, UserPendingRegistration entity)
        {
            command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = entity.Id.Value });
            command.Parameters.Add(new NpgsqlParameter("@LoginUser", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = entity.Login.Value });
            command.Parameters.Add(new NpgsqlParameter("@LoginType", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = entity.Login.Type.ToString() });
            command.Parameters.Add(new NpgsqlParameter("@CountryCode", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = entity.Login switch
                {
                    PhoneLogin phoneLogin => DbValue(phoneLogin.Phone.CountryCode.Value),
                    _ => DBNull.Value
                }
            });
            command.Parameters.Add(new NpgsqlParameter("@Password", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = entity.Password.HashPassword });
            command.Parameters.Add(new NpgsqlParameter("@CreatedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = entity.CreatedAt });
            command.Parameters.Add(new NpgsqlParameter("@ExpiresAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = entity.ExpiresAt });
        }

        private readonly struct UserPendingRegistrationOrdinals
        {
            public int Id { get; }

            public int LoginType { get; }

            public int LoginUser { get; }

            public int CountryCode { get; }

            public int PasswordHash { get; }

            public int ExpiresAt { get; }

            public int CreatedAt { get; }

            public UserPendingRegistrationOrdinals(NpgsqlDataReader reader)
            {
                Id = reader.GetOrdinal("id");
                LoginType = reader.GetOrdinal("login_type");
                LoginUser = reader.GetOrdinal("login_user");
                CountryCode = reader.GetOrdinal("country_code");
                PasswordHash = reader.GetOrdinal("password_hash");
                ExpiresAt = reader.GetOrdinal("expires_at");
                CreatedAt = reader.GetOrdinal("created_at");
            }
        }
    }
}
