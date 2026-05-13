using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.ValueObjects;
using Authorization.Domain.ValueObjects.Login;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserRepository : RepositoryBase, IUserRepository
    {
        public AdoUserRepository(IDbConnectionProvider dbconnectionProvider,
            ILogger<AdoUserRepository> logger)
                : base(logger, dbconnectionProvider) { }

        public async Task<User> CreateAsync(User entity, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        INSERT INTO
                            Users (user_id, login, login_type, password_hash, user_role, created_at, updated_at, last_login_at, last_login_ip, is_confirmed, failed_login_attempts, is_blocked, is_deleted)
                        VALUES
                            (@Id, @Login, @LoginType, @PasswordHash, @Role, NOW(), @UpdatedAt, @LastLoginAt, @LastLoginIp, @IsConfirmed, @FailedLoginAttempts, @IsBlocked, @IsDeleted)
                        RETURNING * ";

                await using (var insertUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(insertUserCommand, entity);

                    await using (var readerInsertUserCommand = await insertUserCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerInsertUserCommand.ReadAsync(cancellationToken))
                        {
                            var newUser = MapReaderToUser(readerInsertUserCommand);

                            _logger.LogInformation("Користувач {Login} збережений в базу. Guid {Id}. Дата створення: {Now}",
                                newUser.Login.Value,
                                newUser.Id,
                                DateTime.UtcNow);

                            return newUser;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(User), entity.Id);
            }, cancellationToken);
        }

        public async Task<User> UpdateAsync(User entity, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        UPDATE 
                            Users 
                        SET 
                            login = @Login, 
                            login_type = @LoginType, 
                            password_hash = @PasswordHash, 
                            user_role = @Role, 
                            updated_at = @UpdatedAt, 
                            last_login_at = @LastLoginAt, 
                            last_login_ip = @LastLoginIp, 
                            is_confirmed = @IsConfirmed, 
                            failed_login_attempts = @FailedLoginAttempts, 
                            is_blocked = @IsBlocked, 
                            is_deleted = @IsDeleted 
                        WHERE
                            user_id = @Id 
                        RETURNING * ";

                await using (var updateUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(updateUserCommand, entity);

                    await using (var readerUpdateUserCommand = await updateUserCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerUpdateUserCommand.ReadAsync(cancellationToken))
                        {
                            var updateUser = MapReaderToUser(readerUpdateUserCommand);

                            _logger.LogInformation("Дані користувача {Login} успішно оновлені. Guid {Id}. Дата оновлення: {Now}",
                                updateUser.Login.Value,
                                updateUser.Id,
                                DateTime.UtcNow);

                            return updateUser;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(User), entity.Id);
            }, cancellationToken);
        }

        public async Task<User> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        DELETE FROM 
                            Users 
                        WHERE 
                            user_id = @Id 
                        RETURNING * ";

                await using (var deletedUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deletedUserCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerDeletedUserCommand = await deletedUserCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerDeletedUserCommand.ReadAsync(cancellationToken))
                        {
                            var userEntity = MapReaderToUser(readerDeletedUserCommand);

                            _logger.LogInformation("Користувача {Login} успішно видалено. Guid {Id}. Дата видалення: {Now}",
                                userEntity.Login.Value,
                                userEntity.Id,
                                DateTime.UtcNow);

                            return userEntity;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(User), id);
            }, cancellationToken);
        }

        public async Task<bool> ExistsByLoginAsync(Login login, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"SELECT EXISTS(SELECT 1 FROM Users WHERE login = @Login)";

                await using (var existsUserCommand = new NpgsqlCommand(sql, conn))
                {
                    existsUserCommand.Parameters.AddWithValue("@Login", login.Value);

                    var result = await existsUserCommand.ExecuteScalarAsync(cancellationToken);

                    return result is bool exists && exists;
                }
            }, cancellationToken);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        SELECT 
                            user_id, login, login_type, password_hash, user_role, created_at, updated_at, last_login_at, last_login_ip, is_confirmed, failed_login_attempts, is_blocked, is_deleted 
                        FROM 
                            Users 
                        WHERE 
                            user_id = @Id ";

                await using (var getUserByIdCommand = new NpgsqlCommand(sql, conn))
                {
                    getUserByIdCommand.Parameters.AddWithValue("@Id", userId);

                    await using (var readerGetUserByIdCommand = await getUserByIdCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerGetUserByIdCommand.ReadAsync(cancellationToken))
                        {
                            var userEntity = MapReaderToUser(readerGetUserByIdCommand);

                            return userEntity;
                        }
                    }
                }

                return null;
            }, cancellationToken);
        }

        public async Task<User?> GetUserByLoginAsync(Login login, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        SELECT 
                            user_id, login, login_type, password_hash, user_role, created_at, updated_at, last_login_at, last_login_ip, is_confirmed, failed_login_attempts, is_blocked, is_deleted 
                        FROM 
                            Users 
                        WHERE 
                            login = @Login ";

                await using (var getUserByLoginCommand = new NpgsqlCommand(sql, conn))
                {
                    getUserByLoginCommand.Parameters.AddWithValue("@Login", login.Value);

                    await using (var readerGetUserByLoginCommand = await getUserByLoginCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerGetUserByLoginCommand.ReadAsync(cancellationToken))
                        {
                            var userEntity = MapReaderToUser(readerGetUserByLoginCommand);

                            return userEntity;
                        }
                    }
                }

                return null;
            }, cancellationToken);
        }

        private User MapReaderToUser(NpgsqlDataReader reader)
        {
            var id = reader.GetGuid("user_id");
            var createdAt = reader.GetDateTime("created_at");

            var loginValue = reader.GetString("login");

            if (!Enum.TryParse<LoginType>(reader.GetString("login_type"), out var loginType))
                throw new DataConsistencyException("Invalid LoginType in DB");

            var login = Login.Restore(loginValue, loginType);

            var passwordHash = Password.Create(reader.GetString("password_hash"));

            if (!Enum.TryParse<Role>(reader.GetString("user_role"), out var role))
                throw new DataConsistencyException("Invalid Role in DB");

            var updatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("updated_at"));
            var lastLoginAt = reader.IsDBNull(reader.GetOrdinal("last_login_at")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("last_login_at"));

            var lastLoginIp = reader.IsDBNull("last_login_ip") ? null : reader.GetString("last_login_ip");

            var isConfirmed = reader.GetBoolean("is_confirmed");
            var failedAttempts = reader.GetInt32("failed_login_attempts");
            var isBlocked = reader.GetBoolean("is_blocked");
            var isDeleted = reader.GetBoolean("is_deleted");

            return User.Restore(id, createdAt, login, passwordHash, role, updatedAt, lastLoginAt, lastLoginIp, isConfirmed, failedAttempts, isBlocked, isDeleted);
        }

        private static void AddParameters(NpgsqlCommand cmd, User entity)
        {
            cmd.Parameters.AddWithValue("@Id", entity.Id);
            cmd.Parameters.AddWithValue("@Login", entity.Login.Value);
            cmd.Parameters.AddWithValue("@LoginType", entity.Login.Type.ToString());
            cmd.Parameters.AddWithValue("@PasswordHash", entity.Password.Value);
            cmd.Parameters.AddWithValue("@Role", entity.Role.ToString());
            cmd.Parameters.AddWithValue("@UpdatedAt", DbValue(entity.UpdatedAt));
            cmd.Parameters.AddWithValue("@LastLoginAt", DbValue(entity.LastLoginAt));
            cmd.Parameters.AddWithValue("@LastLoginIp", DbValue(entity.LastLoginIp));
            cmd.Parameters.AddWithValue("@IsConfirmed", entity.IsLoginConfirmed);
            cmd.Parameters.AddWithValue("@FailedLoginAttempts", entity.FailedLoginAttempts);
            cmd.Parameters.AddWithValue("@IsBlocked", entity.IsBlocked);
            cmd.Parameters.AddWithValue("@IsDeleted", entity.IsDeleted);
        }
    }
}
