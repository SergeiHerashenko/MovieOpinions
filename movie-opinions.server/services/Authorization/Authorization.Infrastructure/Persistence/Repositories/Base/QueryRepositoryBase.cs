using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Base
{
    internal abstract class QueryRepositoryBase<TRepository> : RepositoryBase<TRepository>
    {
        protected readonly IDbConnectionProvider _dbConnectionProvider;

        protected QueryRepositoryBase(
            ILogger<TRepository> logger,
            IDbConnectionProvider dbConnectionProvider)
            : base(logger)
        {
            _dbConnectionProvider = dbConnectionProvider;
        }

        protected Task<T> ExecuteQueryAsync<T>(
            Func<NpgsqlConnection, CancellationToken, Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            return ExecuteQueryCoreAsync(action, cancellationToken);
        }

        protected Task ExecuteQueryAsync(
            Func<NpgsqlConnection, CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            return ExecuteQueryCoreAsync<object?>(
                async (conn, ct) =>
                {
                    await action(conn, ct);
                    return null;
                },
                cancellationToken
            );
        }

        private async Task<TResult> ExecuteQueryCoreAsync<TResult>(
            Func<NpgsqlConnection, CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken)
        {
            try
            {
                await using var connection = await _dbConnectionProvider.GetOpenConnectionAsync(cancellationToken);
                return await action(connection, cancellationToken);
            }
            catch (NpgsqlException ex)
            {
                throw CreateDatabaseException(ex, "query");
            }
        }
    }
}
