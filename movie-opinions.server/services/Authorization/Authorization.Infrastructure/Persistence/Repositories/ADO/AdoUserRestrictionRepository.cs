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
    public class AdoUserRestrictionRepository : RepositoryBase, IUserRestrictionRepository
    {
        public AdoUserRestrictionRepository(IDbConnectionProvider dbConnectionProvider,
            ILogger<AdoUserRestrictionRepository> logger)
                : base(logger, dbConnectionProvider) { }

        public async Task<UserRestriction> CreateAsync(UserRestriction entity, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        INSERT INTO 
                            User_Restrictions (restrictions_id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active) 
                        VALUES
                            (@Id, @UserId, @Login, @Reason, @NameBannedBy, NOW(), @ExpiresAt, @IsActive) 
                        RETURNING * ";

                await using (var restrictionUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(restrictionUserCommand, entity);

                    await using (var readerRestrictionUserCommand = await restrictionUserCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerRestrictionUserCommand.ReadAsync(cancellationToken))
                        {
                            var newRecord = MapReaderToBan(readerRestrictionUserCommand);

                            _logger.LogInformation("Запис про обмеження користувача {Login} збережений в базу. Guid {Id}. Дата створення: {Now}",
                                newRecord.Login,
                                newRecord.Id,
                                DateTime.UtcNow);

                            return newRecord;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserRestriction), entity.Id);
            }, cancellationToken);
        }

        public async Task<UserRestriction> UpdateAsync(UserRestriction entity, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        UPDATE 
                            User_Restrictions 
                        SET 
                            user_id = @UserId,
                            login = @Login,
                            reason = @Reason,
                            name_banned_by = @NameBannedBy,
                            expires_at = @ExpiresAt,
                            is_active = @IsActive
                        WHERE 
                            restrictions_id = @Id
                        RETURNING * ";

                await using (var updateRestrictionUserCommand = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(updateRestrictionUserCommand, entity);

                    await using (var readerUpdateRestrictionUserCommand = await updateRestrictionUserCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerUpdateRestrictionUserCommand.ReadAsync(cancellationToken))
                        {
                            var newUpdateRestrictionUser = MapReaderToBan(readerUpdateRestrictionUserCommand);

                            _logger.LogInformation("Дані заблокованого користувача {Login} успішно оновлені. Guid {Id}. Дата оновлення: {Now}",
                                newUpdateRestrictionUser.Login,
                                newUpdateRestrictionUser.Id,
                                DateTime.UtcNow);

                            return newUpdateRestrictionUser;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserRestriction), entity.Id);
            }, cancellationToken);
        }

        public async Task<UserRestriction> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        DELETE FROM 
                            User_Restrictions
                        WHERE 
                            id = @Id 
                        RETURNING *";

                await using (var deleteRestrictionUserCommand = new NpgsqlCommand(sql, conn))
                {
                    deleteRestrictionUserCommand.Parameters.AddWithValue("@Id", id);

                    await using (var readerDeleteRestrictionUserCommand = await deleteRestrictionUserCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerDeleteRestrictionUserCommand.ReadAsync(cancellationToken))
                        {
                            var deleteRestrictionUser = MapReaderToBan(readerDeleteRestrictionUserCommand);

                            _logger.LogInformation("Запис про користувача {Login} видалено. Guid {Id}. Дата видалення: {Now}",
                                deleteRestrictionUser.Login,
                                deleteRestrictionUser.Id,
                                DateTime.UtcNow);

                            return deleteRestrictionUser;
                        }
                    }
                }

                throw new ReturningNoDataException(nameof(UserRestriction), id);
            }, cancellationToken);
        }

        public async Task<UserRestriction?> GetActiveBanByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        SELECT 
                            restrictions_id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active 
                        FROM 
                            User_Restrictions
                        WHERE 
                            user_id = @UserId
                        AND 
                            is_active = true";

                await using (var getActiveUserRestrictionCommand = new NpgsqlCommand(sql, conn))
                {
                    getActiveUserRestrictionCommand.Parameters.AddWithValue("@UserId", userId);

                    await using (var readerActiveUserRestriction = await getActiveUserRestrictionCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerActiveUserRestriction.ReadAsync(cancellationToken))
                        {
                            var userRestriction = MapReaderToBan(readerActiveUserRestriction);

                            _logger.LogInformation("Заблокований користувач {Login} знайдено!", userRestriction.Login);

                            return userRestriction;
                        }
                    }
                }

                return null;
            }, cancellationToken);
        }

        public async Task<IEnumerable<UserRestriction>> GetAllBansByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var userRestrictionsList = new List<UserRestriction>();

                var sql = @"
                        SELECT 
                            restrictions_id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active 
                        FROM 
                            User_Restrictions
                        WHERE 
                            user_id = @UserId ";

                await using (var getAllUserRestrictionCommand = new NpgsqlCommand(sql, conn))
                {
                    getAllUserRestrictionCommand.Parameters.AddWithValue("@UserId", userId);

                    await using (var readerAllUserRestriction = await getAllUserRestrictionCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await readerAllUserRestriction.ReadAsync(cancellationToken))
                        {
                            var userRestriction = MapReaderToBan(readerAllUserRestriction);

                            userRestrictionsList.Add(userRestriction);
                        }

                        return userRestrictionsList;
                    }
                }
            }, cancellationToken);
        }

        public async Task<UserRestriction?> GetBanByIdAsync(Guid banId, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var sql = @"
                        SELECT 
                            restrictions_id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active 
                        FROM 
                            User_Restrictions
                        WHERE 
                            id = @Id ";

                await using (var getRestrictionCommand = new NpgsqlCommand(sql, conn))
                {
                    getRestrictionCommand.Parameters.AddWithValue("@Id", banId);

                    await using (var readerGetRestrictionCommand = await getRestrictionCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await readerGetRestrictionCommand.ReadAsync(cancellationToken))
                        {
                            var restrictionEntity = MapReaderToBan(readerGetRestrictionCommand);

                            _logger.LogInformation("Запис бану знайдено. Guid {Id}", restrictionEntity.Id);

                            return restrictionEntity;
                        }
                    }
                }

                return null;
            }, cancellationToken);
        }

        public async Task<IEnumerable<UserRestriction>> GetBansByAdminNicknameAsync(string adminNickname, CancellationToken cancellationToken)
        {
            return await ExecuteWithConnectionAsync(async (conn, cancellationToken) =>
            {
                var bannedByAdminList = new List<UserRestriction>();

                var sql = @"
                        SELECT 
                            restrictions_id, user_id, login, reason, name_banned_by, created_at, expires_at, is_active 
                        FROM 
                            User_Restrictions
                        WHERE 
                            name_banned_by = @NameBannedBy";

                await using (var bannedByAdminCommand = new NpgsqlCommand(sql, conn))
                {
                    bannedByAdminCommand.Parameters.AddWithValue("@NameBannedBy", adminNickname);

                    await using (var readerBannedByAdminCommand = await bannedByAdminCommand.ExecuteReaderAsync(cancellationToken))
                    {

                        while (await readerBannedByAdminCommand.ReadAsync(cancellationToken))
                        {
                            var banRecord = MapReaderToBan(readerBannedByAdminCommand);

                            bannedByAdminList.Add(banRecord);
                        }

                        return bannedByAdminList;
                    }
                }
            }, cancellationToken);
        }

        private static void AddParameters(NpgsqlCommand cmd, UserRestriction entity)
        {
            cmd.Parameters.AddWithValue("@Id", entity.Id);
            cmd.Parameters.AddWithValue("@UserId", entity.UserId);
            cmd.Parameters.AddWithValue("@Login", entity.Login);
            cmd.Parameters.AddWithValue("@Reason", DbValue(entity.Reason));
            cmd.Parameters.AddWithValue("@NameBannedBy", entity.NameBannedBy);
            cmd.Parameters.AddWithValue("@ExpiresAt", DbValue(entity.ExpiresAt));
            cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        }

        private UserRestriction MapReaderToBan(NpgsqlDataReader reader)
        {
            var restrictionsId = reader.GetGuid("restrictions_id");
            var userId = reader.GetGuid("user_id");

            var login = reader.GetString("login");

            var reason = reader.IsDBNull("reason") ? null : reader.GetString("reason");

            var nameBannedBy = reader.GetString("name_banned_by");

            var createdAt = reader.GetDateTime("created_at");

            var expiresAt = reader.IsDBNull(reader.GetOrdinal("expires_at")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("expires_at"));

            var isActive = reader.GetBoolean("is_active");

            return UserRestriction.Restore(restrictionsId, userId, login, reason, nameBannedBy, createdAt, expiresAt, isActive);
        }
    }
}
