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
    public class AdoUserPendingAccountChangesRepository : RepositoryBase, IUserPendingAccountChangesRepository
    {
        public AdoUserPendingAccountChangesRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserPendingAccountChangesRepository> logger)
                : base(logger, dbConnectionProvider) { }

        public async Task<UserPendingChange> CreateAsync(UserPendingChange entity, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        INSERT INTO 
                            User_Changes (change_id, user_id, confirmation_token, change_type, new_password_hash, new_login, new_login_type, created_at, expires_at, is_confirmed) 
                        VALUES 
                            (@Id, @UserId, @ConfirmToken, @ChangeType, @NewPasswordHash, @NewLogin, @NewLoginType, NOW(), @ExpiresAt, @IsConfirmed)
                        RETURNING * ";

                await using (var insertChangeCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(insertChangeCommand, entity);

                    await using (var readerInsertChangeCommand = await insertChangeCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerInsertChangeCommand.ReadAsync(cancellationToken))
                        {
                            var changeEntity = MapReaderToChange(readerInsertChangeCommand);

                            _logger.LogInformation("Запис {Id} збережений в базу. Дата створення: {Now}",
                                changeEntity.Id,
                                DateTime.UtcNow);

                            return changeEntity;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserPendingChange), entity.Id);
            }, cancellationToken);
        }

        public async Task<UserPendingChange> UpdateAsync(UserPendingChange entity, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        UPDATE 
                            User_Changes 
                        SET 
                            user_id = @UserId,
                            confirmation_token = @ConfirmToken,
                            change_type = @ChangeType, 
                            new_password_hash = @NewPasswordHash,
                            new_login = @NewLogin,
                            new_login_type = @NewLoginType,
                            expires_at = @ExpiresAt,
                            is_confirmed = @IsConfirmed
                        WHERE 
                            change_id = @Id
                        RETURNING * ";

                await using (var updateChangeCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(updateChangeCommand, entity);

                    await using (var readerChangeCommand = await updateChangeCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerChangeCommand.ReadAsync(cancellationToken))
                        {
                            var updateChange = MapReaderToChange(readerChangeCommand);

                            _logger.LogInformation("Дані оновлені {Id} успішно оновлені. Дата оновлення: {Now}",
                                updateChange.Id,
                                DateTime.UtcNow);

                            return updateChange;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserPendingChange), entity.Id);
            }, cancellationToken);
        }

        public async Task<UserPendingChange> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        DELETE FROM 
                            User_Changes 
                        WHERE
                            change_id = @Id 
                        RETURNING * ";

                await using (var deletedChangeCommand = new NpgsqlCommand(sql, conn))
                {
                    deletedChangeCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerDeletedChangeCommand = await deletedChangeCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerDeletedChangeCommand.ReadAsync(cancellationToken))
                        {
                            var deleteEntity = MapReaderToChange(readerDeletedChangeCommand);

                            _logger.LogInformation("Запис зміни {Id} успішно видалено. Дата видалення: {Now}",
                                deleteEntity.Id,
                                DateTime.UtcNow);

                            return deleteEntity;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserPendingChange), id);
            }, cancellationToken);
        }

        public async Task<UserPendingChange?> GetPendingChangesAsync(Guid id, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        SELECT 
                            change_id, user_id, confirmation_token, change_type, new_password_hash, new_login, new_login_type, created_at, expires_at, is_confirmed 
                        FROM 
                            User_Changes 
                        WHERE 
                            change_id = @Id";

                await using (var getChangeByIdCommand = new NpgsqlCommand(sql, conn))
                {
                    getChangeByIdCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerGeChangeByIdCommand = await getChangeByIdCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerGeChangeByIdCommand.ReadAsync(cancellationToken))
                        {
                            var userEntity = MapReaderToChange(readerGeChangeByIdCommand);

                            return userEntity;
                        }
                    }
                }

                return null;
            }, cancellationToken);
        }

        private static void AddParameters(NpgsqlCommand cmd, UserPendingChange entity)
        {
            cmd.Parameters.AddWithValue("@Id", entity.Id);
            cmd.Parameters.AddWithValue("@UserId", entity.UserId);
            cmd.Parameters.AddWithValue("@ConfirmToken", entity.ConfirmationToken.ToString());
            cmd.Parameters.AddWithValue("@ChangeType", entity.UserChangeType.ToString());
            cmd.Parameters.AddWithValue("@NewPasswordHash", DbValue(entity.NewPassword?.Value));
            cmd.Parameters.AddWithValue("@NewLogin", DbValue(entity.NewLogin?.Value));
            cmd.Parameters.AddWithValue("@NewLoginType", DbValue(entity.NewLogin?.Type.ToString()));
            cmd.Parameters.AddWithValue("@ExpiresAt", entity.ExpiresAt);
            cmd.Parameters.AddWithValue("@IsConfirmed", entity.IsConfirmed);
        }

        private UserPendingChange MapReaderToChange(NpgsqlDataReader reader)
        {
            var id = reader.GetGuid("change_id");
            var userId = reader.GetGuid("user_id");
            var confirmationToken = reader.GetString("confirmation_token");

            if (!Enum.TryParse<UserChangeType>(reader.GetString("change_type"), out var changeType))
                throw new DataConsistencyException("Invalid Change Type in DB");

            Password? password = null;
            if (!reader.IsDBNull("password_hash"))
            {
                password = Password.Create(reader.GetString("new_password_hash"));
            }

            Login? login = null;
            if (!reader.IsDBNull("new_login"))
            {
                var loginValue = reader.GetString("new_login");

                if (!reader.IsDBNull("new_login_type"))
                {
                    if (Enum.TryParse<LoginType>(reader.GetString("new_login_type"), out var loginType))
                    {
                        login = Login.Restore(loginValue, loginType);
                    }
                }
            }

            var createdAt = reader.GetDateTime("created_at");
            var expiresAt = reader.GetDateTime("expires_at");
            var isConfirmed = reader.GetBoolean("is_confirmed");

            return UserPendingChange.Restore(id, userId, confirmationToken, changeType, password, login, expiresAt, isConfirmed, createdAt);
        }
    }
}
