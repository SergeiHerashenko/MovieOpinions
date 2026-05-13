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
    public class AdoUserPendingRegistrationRepository : RepositoryBase, IUserPendingRegistrationRepository
    {
        public AdoUserPendingRegistrationRepository(IDbConnectionProvider dbconnectionProvider,
            ILogger<AdoUserPendingRegistrationRepository> logger)
                : base(logger, dbconnectionProvider) { }

        public async Task<UserPendingRegistration> CreateAsync(UserPendingRegistration entity, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        INSERT INTO 
                            Users_Pending_Registration (user_id, login, login_type, password_hash, created_at, expires_at)
                        VALUES 
                            (@Id, @Login,@LoginType, @PasswordHash, NOW(), @ExpiresAt)
                        RETURNING * ";

                await using (var insertUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(insertUserCommand, entity);

                    await using (var readerInsertUserCommand = await insertUserCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerInsertUserCommand.ReadAsync(cancellationToken))
                        {
                            var newUser = MapReaderToUser(readerInsertUserCommand);

                            _logger.LogInformation("Користувач {Login} збережений в таблицю тимчасовио зареєстрованих. Guid {Id}. Дата створення: {Now}",
                                newUser.Login.Value,
                                newUser.Id,
                                DateTime.UtcNow);

                            return newUser;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserPendingRegistration), entity.Id);
            }, cancellationToken);
        }

        public async Task<UserPendingRegistration> UpdateAsync(UserPendingRegistration entity, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        UPDATE 
                            Users_Pending_Registration 
                        SET 
                            login = @Login, 
                            login_type = @LoginType, 
                            password_hash = @PasswordHash, 
                            expires_at = @ExpiresAt 
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

                throw new ReturningNoDataException(nameof(UserPendingRegistration), entity.Id);
            }, cancellationToken);
        }

        public async Task<UserPendingRegistration> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        DELETE FROM 
                            Users_Pending_Registration 
                        WHERE 
                            user_id = @Id 
                        RETURNING * ";

                await using (var deleteUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deleteUserCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerDeletedUserCommand = await deleteUserCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerDeletedUserCommand.ReadAsync(cancellationToken))
                        {
                            var userEntity = MapReaderToUser(readerDeletedUserCommand);

                            _logger.LogInformation("Користувача {Login} успішно видалено з таблиці реєстрації. Guid {Id}. Дата видалення: {Now}",
                                userEntity.Login.Value,
                                userEntity.Id,
                                DateTime.UtcNow);

                            return userEntity;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserPendingRegistration), id);
            }, cancellationToken);
        }

        public async Task<UserPendingRegistration?> GetByLoginAsync(Login login, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        SELECT 
                            user_id, login, login_type, password_hash, created_at, expires_at 
                        FROM 
                            Users_Pending_Registration
                        WHERE 
                            login = @Login";

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

        private UserPendingRegistration MapReaderToUser(NpgsqlDataReader reader)
        {
            var id = reader.GetGuid("user_id");

            var loginValue = reader.GetString("login");
            if (!Enum.TryParse<LoginType>(reader.GetString("login_type"), out var loginType))
                throw new DataConsistencyException("Invalid LoginType in DB");

            var login = Login.Restore(loginValue, loginType);

            var passwordHash = Password.Create(reader.GetString("password_hash"));

            var createdAt = reader.GetDateTime("created_at");
            var expiresAt = reader.GetDateTime("expires_at");

            return UserPendingRegistration.Restore(id, login, passwordHash, createdAt, expiresAt);
        }

        private static void AddParameters(NpgsqlCommand cmd, UserPendingRegistration entity)
        {
            cmd.Parameters.AddWithValue("@Id", entity.Id);
            cmd.Parameters.AddWithValue("@Login", entity.Login.Value);
            cmd.Parameters.AddWithValue("@LoginType", entity.Login.Type.ToString());
            cmd.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash.Value);
            cmd.Parameters.AddWithValue("@ExpiresAt", entity.ExpiresAt);
        }
    }
}
