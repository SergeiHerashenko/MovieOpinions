using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories
{
    public abstract class RepositoryBase
    {
        protected readonly ILogger _logger;

        private readonly IDbConnectionProvider _dbConnectionProvider;

        public RepositoryBase(
            ILogger<RepositoryBase> logger, IDbConnectionProvider dbConnectionProvider)
        {
            _logger = logger;
            _dbConnectionProvider = dbConnectionProvider;
        }

        protected static object DbValue(object? value) => value ?? DBNull.Value;

        protected async Task<T> ExecuteWithConnectionAsync<T>(
            Func<NpgsqlConnection, CancellationToken, Task<T>> action, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using var connection = await _dbConnectionProvider.GetOpenConnectionAsync(cancellationToken);

                return await action(connection, cancellationToken);
            }
            catch(NpgsqlException ex)
            {
                _logger.LogCritical(ex, "Критична помилка Postgres: {State}", ex.SqlState);

                throw DatabaseOperationException.NoConnection(
                    message: "Помилка бази даних",
                    innerException: ex
                );
            }
        }
    }
}
