using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Domain.Users;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserRepository : RepositoryBase, IUserRepository
    {
        private readonly IClock _clock;

        public AdoUserRepository(
            IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserRepository> logger,
            IClock clock)
            : base(logger, dbConnectionProvider)
        {
            _clock = clock;
        }

        public async Task<User> CreateAsync(User entity, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"
                            INSERT INTO 
                                User_Account (id, login_user, login_type, country_code, password_hash, role, updated_at, last_login_at, last_login_ip, is_login_confirmed, failed_login_attempts, is_blocked, is_deleted, created_at) 
                            VALUES 
                                (@Id, @LoginUser, @LoginType, @CountryCode, @Password, @Role, @UpdatedAt, @LastLoginAt, @LastLoginIp, @IsLoginConfirmed, @FailedLoginAttempts, @IsBlocked, @IsDeleted, @CreatedAt) 
                            RETURNING *; ";

                await using (var insertUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(insertUserCommand, entity);
                    insertUserCommand.Parameters.Add(new NpgsqlParameter("@CreatedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = entity.CreatedAt });

                    await using (var readerInsertUserCommand = await insertUserCommand.ExecuteReaderAsync(ct))
                    {
                        if(await readerInsertUserCommand.ReadAsync(ct))
                        {
                            var ords = new UserOrdinals(readerInsertUserCommand);
                            var newUser = MapReaderToUser(readerInsertUserCommand, ords);

                            _logger.LogInformation("User {Login} saved to main table. Guid {Id}. Creation date: {Now}",
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
                        ["Entity"] = nameof(User),
                        ["Method"] = nameof(CreateAsync),
                        ["Login"] = entity.Login.Value,
                        ["Id"] = entity.Id.Value,
                        ["Date"] = _clock.UtcNow
                    }
                );
            }, cancellationToken);
        }

        public async Task<User> UpdateAsync(User entity, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"
                            UPDATE
                                User_Account 
                            SET 
                                login_user = @LoginUser, 
                                login_type = @LoginType, 
                                country_code = @CountryCode, 
                                password_hash = @Password, 
                                role = @Role,
                                updated_at = @UpdatedAt, 
                                last_login_at = @LastLoginAt, 
                                last_login_ip = @LastLoginIp, 
                                is_login_confirmed = @IsLoginConfirmed, 
                                failed_login_attempts = @FailedLoginAttempts, 
                                is_blocked = @IsBlocked, 
                                is_deleted = @IsDeleted 
                            WHERE 
                                id = @Id 
                            RETURNING *;";

                await using (var updateUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(updateUserCommand, entity);

                    await using (var readerUpdateUserCommand = await updateUserCommand.ExecuteReaderAsync(ct))
                    {
                        if(await readerUpdateUserCommand.ReadAsync(ct))
                        {
                            var ords = new UserOrdinals(readerUpdateUserCommand);
                            var updateUser = MapReaderToUser(readerUpdateUserCommand, ords);

                            _logger.LogInformation("User {Login} data successfully updated. Guid {Id}. Update date: {Now}",
                                updateUser.Login.Value,
                                updateUser.Id.Value,
                                _clock.UtcNow
                            );

                            return updateUser;
                        }
                    }
                }

                throw ReturningNoDataException.NoDataReceived(
                    $"After updating the database, no user data was returned for {entity.Login.Value}!",
                    new Dictionary<string, object>
                    {
                        ["Entity"] = nameof(User),
                        ["Method"] = nameof(UpdateAsync),
                        ["Login"] = entity.Login.Value,
                        ["Id"] = entity.Id.Value,
                        ["Date"] = _clock.UtcNow
                    }
                );
            }, cancellationToken);
        }

        public async Task<User> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"
                            DELETE FROM 
                                User_Account 
                            WHERE 
                                id = @Id 
                            RETURNING *;";

                await using (var deletedUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deletedUserCommand.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = id });

                    await using (var readerDeletedUserCommand = await deletedUserCommand.ExecuteReaderAsync(ct))
                    {
                        if(await readerDeletedUserCommand.ReadAsync(ct))
                        {
                            var ords = new UserOrdinals(readerDeletedUserCommand);
                            var userEntity = MapReaderToUser(readerDeletedUserCommand, ords);

                            _logger.LogInformation("User {Login} successfully deleted. Guid {Id}. Deleted on: {Now}",
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
                        ["Entity"] = nameof(User),
                        ["Method"] = nameof(DeleteAsync),
                        ["Id"] = id,
                        ["Date"] = _clock.UtcNow
                    }
                );
            }, cancellationToken);
        }

        public async Task<bool> ExistsByLoginAsync(Login login, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"SELECT EXISTS(SELECT 1 FROM User_Account WHERE login_user = @LoginUser)";

                await using (var existsUserCommand = new NpgsqlCommand(sql, conn))
                {
                    existsUserCommand.Parameters.Add(new NpgsqlParameter("@LoginUser", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = login.Value });

                    var result = await existsUserCommand.ExecuteScalarAsync(ct);

                    return result is bool exists && exists;
                }
            }, cancellationToken);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"
                            SELECT 
                                id, login_user, login_type, country_code, password_hash, role, updated_at, last_login_at, last_login_ip, is_login_confirmed, failed_login_attempts, is_blocked, is_deleted, created_at 
                            FROM 
                                User_Account 
                            WHERE 
                                id = @Id;";

                await using (var getUserByIdCommand = new NpgsqlCommand(sql, conn))
                {
                    getUserByIdCommand.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = userId });

                    await using (var readerGetUserByIdCommand = await getUserByIdCommand.ExecuteReaderAsync(ct))
                    {
                        if (await readerGetUserByIdCommand.ReadAsync(ct))
                        {
                            var ords = new UserOrdinals(readerGetUserByIdCommand);
                            var userEntity = MapReaderToUser(readerGetUserByIdCommand, ords);

                            return userEntity;
                        }
                    }
                }

                return null;
            }, cancellationToken);
        }

        public async Task<User?> GetUserByLoginAsync(Login login, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"
                            SELECT 
                                id, login_user, login_type, country_code, password_hash, role, updated_at, last_login_at, last_login_ip, is_login_confirmed, failed_login_attempts, is_blocked, is_deleted, created_at 
                            FROM 
                                User_Account 
                            WHERE 
                                login_user = @LoginUser;";

                await using (var getUserByLoginCommand = new NpgsqlCommand(sql, conn))
                {
                    getUserByLoginCommand.Parameters.Add(new NpgsqlParameter("@LoginUser", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = login.Value });

                    await using (var readerGetUserByLoginCommand = await getUserByLoginCommand.ExecuteReaderAsync(ct))
                    {
                        if (await readerGetUserByLoginCommand.ReadAsync(ct))
                        {
                            var ords = new UserOrdinals(readerGetUserByLoginCommand);
                            var userEntity = MapReaderToUser(readerGetUserByLoginCommand, ords);

                            return userEntity;
                        }
                    }
                }

                return null;
            }, cancellationToken);
        }

        private User MapReaderToUser(NpgsqlDataReader reader, in UserOrdinals ords)
        {
            var id = UserId.Restore(reader.GetGuid(ords.Id));
            var createdAt = reader.GetFieldValue<DateTimeOffset>(ords.CreatedAt);

            if (!Enum.TryParse<LoginType>(reader.GetString(ords.LoginType), out var loginType))
                throw DataConsistencyException.UnknownType(
                    $"Unknown type for {nameof(LoginType)}",
                    new Dictionary<string, object>
                    {
                        ["LoginType"] = reader.GetString(ords.LoginType),
                        ["Entity"] = nameof(User),
                        ["Id"] = id
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
                            ["Entity"] = nameof(User),
                            ["Id"] = id
                        }
                    )
            };

            var password = Password.Restore(reader.GetString(ords.PasswordHash));

            if (!Enum.TryParse<Role>(reader.GetString(ords.Role), out var role))
                throw DataConsistencyException.UnknownType(
                    $"Unknown type for {nameof(Role)}",
                    new Dictionary<string, object>
                    {
                        ["Role"] = reader.GetString(ords.Role),
                        ["Entity"] = nameof(User),
                        ["Id"] = id
                    }
                );

            var updatedAt = reader.IsDBNull(ords.UpdatedAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ords.UpdatedAt);
            var lastLoginAt = reader.IsDBNull(ords.LastLoginAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ords.LastLoginAt);

            var lastLoginIp = reader.IsDBNull(ords.LastLoginIp) ? null : reader.GetString(ords.LastLoginIp);

            var isConfirmed = reader.GetBoolean(ords.IsLoginConfirmed);
            var failedAttempts = reader.GetInt32(ords.FailedLoginAttempts);
            var isBlocked = reader.GetBoolean(ords.IsBlocked);
            var isDeleted = reader.GetBoolean(ords.IsDeleted);

            return User.Restore(id, createdAt, login, password, role, updatedAt, lastLoginAt, lastLoginIp, isConfirmed, failedAttempts, isBlocked, isDeleted);
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
                        ["Entity"] = nameof(User),
                        ["FullNumber"] = fullNumber
                    }
                );

            var phoneNumber = fullNumber.StartsWith(countryCode)
                ? fullNumber.Substring(countryCode.Length)
                : fullNumber;

            return PhoneLogin.Restore(countryCode, phoneNumber);
        }

        private static void AddParameters(NpgsqlCommand command, User entity)
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
            command.Parameters.Add(new NpgsqlParameter("@Role", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = entity.Role.ToString() });
            command.Parameters.Add(new NpgsqlParameter("@UpdatedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = DbValue(entity.UpdatedAt) });
            command.Parameters.Add(new NpgsqlParameter("@LastLoginAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = DbValue(entity.LastLoginAt) });
            command.Parameters.Add(new NpgsqlParameter("@LastLoginIp", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = DbValue(entity.LastLoginIp) });
            command.Parameters.Add(new NpgsqlParameter("@IsLoginConfirmed", NpgsqlTypes.NpgsqlDbType.Boolean) { Value = entity.IsLoginConfirmed });
            command.Parameters.Add(new NpgsqlParameter("@FailedLoginAttempts", NpgsqlTypes.NpgsqlDbType.Integer) { Value = entity.FailedLoginAttempts });
            command.Parameters.Add(new NpgsqlParameter("@IsBlocked", NpgsqlTypes.NpgsqlDbType.Boolean) { Value = entity.IsBlocked });
            command.Parameters.Add(new NpgsqlParameter("@IsDeleted", NpgsqlTypes.NpgsqlDbType.Boolean) { Value = entity.IsDeleted });
        }

        private readonly struct UserOrdinals
        {
            public int Id { get; }

            public int LoginType { get; }

            public int LoginUser { get; }

            public int CountryCode { get; }

            public int PasswordHash { get; }

            public int Role { get; }

            public int CreatedAt { get; }

            public int UpdatedAt { get; }

            public int LastLoginAt { get; }

            public int LastLoginIp { get; }

            public int IsLoginConfirmed { get; }

            public int FailedLoginAttempts { get; }

            public int IsBlocked { get; }

            public int IsDeleted { get; }

            public UserOrdinals(NpgsqlDataReader reader)
            {
                Id = reader.GetOrdinal("id");
                LoginType = reader.GetOrdinal("login_type");
                LoginUser = reader.GetOrdinal("login_user");
                CountryCode = reader.GetOrdinal("country_code");
                PasswordHash = reader.GetOrdinal("password_hash");
                Role = reader.GetOrdinal("role");
                CreatedAt = reader.GetOrdinal("created_at");
                UpdatedAt = reader.GetOrdinal("updated_at");
                LastLoginAt = reader.GetOrdinal("last_login_at");
                LastLoginIp = reader.GetOrdinal("last_login_ip");
                IsLoginConfirmed = reader.GetOrdinal("is_login_confirmed");
                FailedLoginAttempts = reader.GetOrdinal("failed_login_attempts");
                IsBlocked = reader.GetOrdinal("is_blocked");
                IsDeleted = reader.GetOrdinal("is_deleted");
            }
        }
    }
}
