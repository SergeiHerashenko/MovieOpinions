using Authorization.Domain.Users;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.Base;
using Authorization.Infrastructure.Persistence.Repositories.Common.AdoMappers;
using Authorization.Infrastructure.Persistence.Repositories.Common.Ordinals;
using Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado.Loading;
using Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado.Models;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado
{
    internal class AdoUserQueryRepository : QueryRepositoryBase<AdoUserQueryRepository>
    {
        private readonly AdoUserAggregateLoader _adoUserAggregateLoader;

        public AdoUserQueryRepository(
            ILogger<AdoUserQueryRepository> logger,
            IDbConnectionProvider dbConnectionProvider,
            AdoUserAggregateLoader adoUserAggregateLoader)
            : base(logger, dbConnectionProvider)
        {
            _adoUserAggregateLoader = adoUserAggregateLoader;
        }

        internal async Task<User?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var userData = await LoadUserDataByIdAsync(connection, userId, ct);

                if (userData is null)
                    return null;

                return await _adoUserAggregateLoader.BuildUserAggregateAsync(connection, transaction: null, userData, ct);
            }, cancellationToken);
        }

        internal async Task<User?> GetUserByLoginAsync(Login login, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var userData = await LoadUserDataByLoginAsync(connection, login, ct);

                if (userData is null)
                    return null;

                return await _adoUserAggregateLoader.BuildUserAggregateAsync(connection, transaction: null, userData, ct);
            }, cancellationToken);
        }

        internal async Task<bool> ExistsUserByLoginAsync(Login login, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(async (connection, ct) =>
            {
                var sql = @"SELECT EXISTS(SELECT 1 FROM User_Account WHERE login = @Login)";

                await using var command = new NpgsqlCommand(sql, connection);

                command.Parameters.Add(new NpgsqlParameter("@Login", NpgsqlDbType.Varchar) { Value = login.Value });

                var result = await command.ExecuteScalarAsync(ct);

                return (bool)result!;
            }, cancellationToken);
        }

        #region Aggregate loading
        private async Task<UserData?> LoadUserDataByLoginAsync(NpgsqlConnection connection, Login login, CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT 
                            id, created_at, login_type, login, country_code, email_domain, password_hash, role, updated_at, last_login_at, is_login_confirmed, failed_password_attempts 
                        FROM 
                            User_Account 
                        WHERE 
                            login = @Login;";

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.Add(new NpgsqlParameter("@Login", NpgsqlDbType.Varchar) { Value = login.Value });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            var ordinals = new UserOrdinals(reader);

            return UserDataMapper.Map(reader, ordinals);
        }

        private async Task<UserData?> LoadUserDataByIdAsync(NpgsqlConnection connection, UserId userId, CancellationToken cancellationToken = default)
        {
            var sql = @"SELECT 
                            id, created_at, login_type, login, country_code, email_domain, password_hash, role, updated_at, last_login_at, is_login_confirmed, failed_password_attempts 
                        FROM 
                            User_Account 
                        WHERE 
                            id = @Id;";

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.Add(new NpgsqlParameter("@Id", NpgsqlDbType.Uuid) { Value = userId.Value });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            var ordinals = new UserOrdinals(reader);

            return UserDataMapper.Map(reader, ordinals);
        }
        #endregion
    }
}
