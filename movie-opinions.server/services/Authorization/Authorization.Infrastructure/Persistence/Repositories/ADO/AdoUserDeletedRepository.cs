using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersDeletion;
using Authorization.Domain.UsersDeletion.Enums;
using Authorization.Domain.UsersDeletion.ValueObjects;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserDeletedRepository : RepositoryBase, IUserDeletedRepository
    {
        private readonly IClock _clock;

        public AdoUserDeletedRepository(
            ILogger<RepositoryBase> logger, 
            IDbConnectionProvider dbConnectionProvider,
            IClock clock) 
            : base(logger, dbConnectionProvider)
        {
            _clock = clock;
        }

        public async Task<UserDeletion> CreateAsync(UserDeletion entity, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"
                            INSERT INTO 
                                User_Deleted (id, user_id, login_user, login_type, country_code, reason, deleted_at, restore_until, restored_at, status, updated_at, created_at) 
                            VALUES 
                                (@Id, @UserId, @LoginUser, @LoginType, @CountryCode, @Reason, @DeletedAt, @RestoreUntil, @RestoredAt, @Status, @UpdatedAt, @CreatedAt) 
                            RETURNING *; ";
                
                await using (var command = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(command, entity);
                    command.Parameters.Add(new NpgsqlParameter("@CreatedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = entity.CreatedAt });

                    await using (var reader = await command.ExecuteReaderAsync(ct))
                    {
                        if(await reader.ReadAsync(ct))
                        {
                            var ords = new UserDeletedOrdinals(reader);
                            var createdEntity = MapReaderToUserDeletion(reader, ords);

                            _logger.LogInformation("User {Login} saved in delete table. Guid {Id}. Creation date: {Now}",
                                createdEntity.Login.Value,
                                createdEntity.Id.Value,
                                _clock.UtcNow
                            );

                            return createdEntity;
                        }
                    }
                }

                throw ReturningNoDataException.NoDataReceived(
                    $"No user data was returned for {entity.Login.Value} after inserting the deleted users into the database!",
                    new Dictionary<string, object>
                    {
                        ["Entity"] = nameof(UserDeletion),
                        ["Method"] = nameof(CreateAsync),
                        ["Login"] = entity.Login.Value,
                        ["Id"] = entity.Id.Value,
                        ["Date"] = _clock.UtcNow
                    }
                );
            }, cancellationToken);
        }

        public async Task<UserDeletion> UpdateAsync(UserDeletion entity, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                // TODO дописати команду
                var sql = @"
                            UPDATE
                                User_Deleted 
                            SET 
                                ,";

                await using (var command = new NpgsqlCommand(sql, conn))
                {
                    AddParameters(command, entity);

                    await using (var reader = await command.ExecuteReaderAsync(ct))
                    {
                        if(await reader.ReadAsync(ct))
                        {
                            var ords = new UserDeletedOrdinals(reader);
                            var updateEntity = MapReaderToUserDeletion(reader, ords);

                            _logger.LogInformation("User {Login} data successfully updated, table {TableName}. Guid {Id}. Update date: {Now}",
                                updateEntity.Login.Value,
                                nameof(UserDeletion),
                                updateEntity.Id.Value,
                                _clock.UtcNow
                            );

                            return updateEntity;
                        }
                    }
                }

                throw ReturningNoDataException.NoDataReceived(
                    $"After updating the database, table {nameof(UserDeletion)} no user data was returned for {entity.Login.Value}!",
                    new Dictionary<string, object>
                    {
                        ["Entity"] = nameof(UserDeletion),
                        ["Method"] = nameof(UpdateAsync),
                        ["Login"] = entity.Login.Value,
                        ["Id"] = entity.Id.Value,
                        ["Date"] = _clock.UtcNow
                    }
                );
            });
        }

        public async Task<UserDeletion> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithConnectionAsync(async (conn, ct) =>
            {
                var sql = @"
                            DELETE FROM 
                                User_Deleted 
                            WHERE 
                                id = @Id 
                            RETURNING *; ";

                await using (var command = new NpgsqlCommand(sql, conn))
                {
                    command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = id});

                    await using (var reader = await command.ExecuteReaderAsync(ct))
                    {
                        if(await reader.ReadAsync(ct))
                        {
                            var ords = new UserDeletedOrdinals(reader);
                            var deleteEntity = MapReaderToUserDeletion(reader, ords);

                            _logger.LogInformation("User {Login} successfully deleted from table {NameTable}. Guid {Id}. Deleted: {Now}!",
                                deleteEntity.Login.Value,
                                nameof(UserDeletion),
                                deleteEntity.Id.Value,
                                _clock.UtcNow
                            );

                            return deleteEntity;
                        }
                    }
                }

                throw ReturningNoDataException.NoDataReceived(
                    $"No data was received for the deleted user {id}!",
                    new Dictionary<string, object>
                    {
                        ["Entity"] = nameof(UserDeletion),
                        ["Method"] = nameof(DeleteAsync),
                        ["Id"] = id,
                        ["Date"] = _clock.UtcNow
                    }
                );
            }, cancellationToken);
        }

        public Task<UserDeletion?> GetDeletionUserById(UserId userId)
        {
            throw new NotImplementedException();
        }

        private UserDeletion MapReaderToUserDeletion(NpgsqlDataReader reader, in UserDeletedOrdinals ords)
        {
            var id = UserDeletionId.Restore(reader.GetGuid(ords.Id));
            var userId = UserId.Restore(reader.GetGuid(ords.UserId));
            var createdAt = reader.GetFieldValue<DateTimeOffset>(ords.CreatedAt);

            if (!Enum.TryParse<LoginType>(reader.GetString(ords.LoginType), out var loginType))
                throw DataConsistencyException.UnknownType(
                    $"Unknow type for {nameof(LoginType)}",
                    new Dictionary<string, object>
                    {
                        ["LoginType"] = reader.GetString(ords.LoginType),
                        ["Id"] = id,
                        ["Entity"] = nameof(UserDeletion)
                    }
                );

            Login login = loginType switch
            {
                LoginType.Email => EmailLogin.Restore(reader.GetString(ords.LoginUser)),
                LoginType.Phone => RestorePhoneLogin(reader, ords.CountryCode, reader.GetString(ords.LoginUser)),
                _ => throw DataConsistencyException.UnknownType(
                        $"Unknow type for {nameof(LoginType)}",
                        new Dictionary<string, object>
                        {
                            ["LoginType"] = reader.GetString(ords.LoginType),
                            ["Id"] = id,
                            ["Entity"] = nameof(UserDeletion)
                        }
                    )
            };

            var reason = reader.IsDBNull(ords.Reason) ? null : reader.GetString(ords.Reason);

            var deletedAt = reader.GetFieldValue<DateTimeOffset>(ords.DeletedAt);
            var restoreUntil = reader.GetFieldValue<DateTimeOffset>(ords.RestoreUntil);
            var restoredAt = reader.IsDBNull(ords.RestoredAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ords.RestoredAt);

            if (!Enum.TryParse<DeletionStatus>(reader.GetString(ords.Status), out var deletionStatus))
                throw DataConsistencyException.UnknownType(
                    $"Unknow type for {nameof(DeletionStatus)}",
                    new Dictionary<string, object>
                    {
                        ["DeletionStatus"] = reader.GetString(ords.Status),
                        ["Id"] = id,
                        ["Entity"] = nameof(UserDeletion)
                    }
                );

            var updatedAt = reader.IsDBNull(ords.UpdatedAt) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(ords.UpdatedAt);

            return UserDeletion.Restore(id, userId, login, reason, deletedAt, createdAt, restoreUntil, restoredAt, deletionStatus, updatedAt);
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
                        ["Entity"] = nameof(UserDeletion),
                        ["FullNumber"] = fullNumber
                    }
                );

            var phoneNumber = fullNumber.StartsWith(countryCode)
                ? fullNumber.Substring(countryCode.Length)
                : fullNumber;

            return PhoneLogin.Restore(countryCode, phoneNumber);
        }

        private static void AddParameters(NpgsqlCommand command, UserDeletion entity)
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
            command.Parameters.Add(new NpgsqlParameter("@Reason", NpgsqlTypes.NpgsqlDbType.Text) { Value = DbValue(entity.Reason)});
            command.Parameters.Add(new NpgsqlParameter("@DeletedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = entity.DeletedAt});
            command.Parameters.Add(new NpgsqlParameter("@RestoreUntil", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = entity.RestoreUntil});
            command.Parameters.Add(new NpgsqlParameter("@RestoredAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = DbValue(entity.RestoredAt)});
            command.Parameters.Add(new NpgsqlParameter("@Status", NpgsqlTypes.NpgsqlDbType.Text) { Value = entity.Status.ToString()});
            command.Parameters.Add(new NpgsqlParameter("@UpdatedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = DbValue(entity.UpdatedAt)});
        }

        private readonly struct UserDeletedOrdinals
        {
            public int Id { get; }

            public int UserId { get; }

            public int LoginUser { get; }

            public int LoginType { get; }

            public int CountryCode { get; }

            public int Reason { get; }

            public int DeletedAt { get; }

            public int RestoreUntil { get; }

            public int RestoredAt { get; }

            public int Status { get; }

            public int UpdatedAt { get; }

            public int CreatedAt { get; }

            public UserDeletedOrdinals(NpgsqlDataReader reader)
            {
                Id = reader.GetOrdinal("id");
                UserId = reader.GetOrdinal("user_id");
                LoginUser = reader.GetOrdinal("login_user");
                LoginType = reader.GetOrdinal("login_type");
                CountryCode = reader.GetOrdinal("country_code");
                Reason = reader.GetOrdinal("reason");
                DeletedAt = reader.GetOrdinal("deleted_at");
                RestoreUntil = reader.GetOrdinal("restore_until");
                RestoredAt = reader.GetOrdinal("restored_at");
                Status = reader.GetOrdinal("status");
                UpdatedAt = reader.GetOrdinal("update_at");
                CreatedAt = reader.GetOrdinal("created_at");
            }
        }
    }
}
