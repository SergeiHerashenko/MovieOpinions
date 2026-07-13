using Authorization.Application.Interfaces.Persistence;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersDeletion;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.ADO
{
    public class AdoUserDeletedRepository : RepositoryBase, IUserDeletedRepository
    {
        public AdoUserDeletedRepository(
            ILogger<RepositoryBase> logger, 
            IDbConnectionProvider dbConnectionProvider) 
            : base(logger, dbConnectionProvider)
        {

        }

        public Task<UserDeletion> CreateAsync(UserDeletion entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UserDeletion> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UserDeletion?> GetDeletionUserById(UserId userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserDeletion> UpdateAsync(UserDeletion entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        //private UserDeletion MapReaderToUserDeletion(NpgsqlDataReader reader, in UserDeletedOrdinals ordinals)
        //{
        //
        //}

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
            command.Parameters.Add(new NpgsqlParameter("@CreatedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = entity.CreatedAt});
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
