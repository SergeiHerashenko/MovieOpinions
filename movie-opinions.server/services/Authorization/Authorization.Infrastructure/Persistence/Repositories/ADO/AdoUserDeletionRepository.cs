using Authorization.Application.Interfaces.Repositories;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserDeletionRepository : RepositoryBase, IUserDeletionRepository
    {
        public AdoUserDeletionRepository(IDbConnectionProvider dbconnectionProvider,
            ILogger<AdoUserDeletionRepository> logger)
                : base(logger, dbconnectionProvider) { }

        public async Task<UserDeletion> CreateAsync(UserDeletion entity, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        INSERT INTO 
                            Users_Deleted  (deletion_id, user_id, login, reason, deleted_at) 
                        VALUES 
                            (@Id, @UserId, @Login, @Reason, NOW()) 
                        RETURNING * ";

                await using (var deletedUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(deletedUserCommand, entity);

                    await using (var readerDeletedUserCommand = await deletedUserCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerDeletedUserCommand.ReadAsync(cancellationToken))
                        {
                            var newDeletedUser = MapReaderToDeleteUser(readerDeletedUserCommand);

                            _logger.LogInformation("Видалений користувач {Login}, був успішно створений. Guid {Id}. Дата створення: {Now}",
                                newDeletedUser.Login,
                                newDeletedUser.Id,
                                DateTime.UtcNow);

                            return newDeletedUser;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserDeletion), entity.Id);
            }, cancellationToken);
        }

        public async Task<UserDeletion> UpdateAsync(UserDeletion entity, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        UPDATE 
                            Users_Deleted 
                        SET 
                            user_id = @UserId, 
                            login = @Login,
                            reason = @Reason
                        WHERE 
                            deletion_id = @Id
                        RETURNING * ";

                await using (var updateDeletedUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(updateDeletedUserCommand, entity);

                    await using (var readerUpdateDeletedUserCommand = await updateDeletedUserCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerUpdateDeletedUserCommand.ReadAsync(cancellationToken))
                        {
                            var newUpdateDeletedUser = MapReaderToDeleteUser(readerUpdateDeletedUserCommand);

                            _logger.LogInformation("Інформація користувача {Login}, була успішно оновлена. Guid {Id}. Дата оновлення: {Now}",
                                newUpdateDeletedUser.Login,
                                newUpdateDeletedUser.Id,
                                DateTime.UtcNow);

                            return newUpdateDeletedUser;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserDeletion), entity.Id);
            }, cancellationToken);
        }

        public async Task<UserDeletion> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        DELETE FROM 
                            Users_Deleted
                        WHERE 
                            user_id = @UserId
                        RETURNING * ";

                await using (var deletedUserRecordCommand = new NpgsqlCommand(sql, conn))
                {
                    deletedUserRecordCommand.Parameters.AddWithValue("@UserId", id);

                    await using (var readerDeletedUserRecordCommand = await deletedUserRecordCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerDeletedUserRecordCommand.ReadAsync(cancellationToken))
                        {
                            var deletedRecordUser = MapReaderToDeleteUser(readerDeletedUserRecordCommand);

                            _logger.LogInformation("Інформація видаленого користувач {Login}, була успішно видалена. Guid {Id}. Дата видалення: {Now}",
                                deletedRecordUser.Login,
                                deletedRecordUser.Id,
                                DateTime.UtcNow);

                            return deletedRecordUser;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserDeletion), id);
            }, cancellationToken);
        }

        public async Task<UserDeletion?> GetUserDeletionsByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        SELECT 
                            deletion_id, user_id, login, reason, deleted_at 
                        FROM 
                            Users_Deleted 
                        WHERE 
                            user_id = @UserId ";

                await using (var getUserDeletionByIdCommand = new NpgsqlCommand(sql, conn))
                {
                    getUserDeletionByIdCommand.Parameters.AddWithValue("@UserId", userId);

                    await using (var readerDeletedUserByIdCommand = await getUserDeletionByIdCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerDeletedUserByIdCommand.ReadAsync(cancellationToken))
                        {
                            var deletedRecordUser = MapReaderToDeleteUser(readerDeletedUserByIdCommand);

                            _logger.LogInformation("Інформація про користувача {Login}, знайдена!", deletedRecordUser.Login);

                            return deletedRecordUser;
                        }
                    }
                }

                return null;
            }, cancellationToken);
        }

        public async Task<UserDeletion?> GetUserDeletionsByLoginAsync(string login, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        SELECT 
                            deletion_id, user_id, login, reason, deleted_at 
                        FROM 
                            Users_Deleted 
                        WHERE 
                            login = @Login ";

                await using (var getUserDeletionByLoginCommand = new NpgsqlCommand(sql, conn))
                {
                    getUserDeletionByLoginCommand.Parameters.AddWithValue("@Login", login);

                    await using (var readerDeletedUserByLoginCommand = await getUserDeletionByLoginCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerDeletedUserByLoginCommand.ReadAsync(cancellationToken))
                        {
                            var deletedRecordUser = MapReaderToDeleteUser(readerDeletedUserByLoginCommand);

                            _logger.LogInformation("Інформація про користувача {Login}, знайдена!", deletedRecordUser.Login);

                            return deletedRecordUser;
                        }
                    }
                }

                return null;
            }, cancellationToken);
        }

        private UserDeletion MapReaderToDeleteUser(NpgsqlDataReader reader)
        {
            var id = reader.GetGuid("deletion_id");
            var userId = reader.GetGuid("user_id");
            var login = reader.GetString("login");
            var reason = reader.IsDBNull("reason") ? null : reader.GetString("reason");
            var createdAt = reader.GetDateTime("deleted_at");

            return UserDeletion.Restore(id, userId, login, reason, createdAt);
        }

        private static void AddParameters(NpgsqlCommand cmd, UserDeletion entity)
        {
            cmd.Parameters.AddWithValue("@Id", entity.Id);
            cmd.Parameters.AddWithValue("@UserId", entity.UserId);
            cmd.Parameters.AddWithValue("@Login", entity.Login);
            cmd.Parameters.AddWithValue("@Reason", DbValue(entity.Reason));
        }
    }
}