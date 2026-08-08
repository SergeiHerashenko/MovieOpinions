using Authorization.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Base
{
    internal abstract class RepositoryBase<TRepository>
    {
        protected readonly ILogger<TRepository> _logger;

        public RepositoryBase(ILogger<TRepository> logger)
        {
            _logger = logger;
        }

        protected static object DbValue(object? value) => value ?? DBNull.Value;

       protected DatabaseOperationException CreateDatabaseException(NpgsqlException ex, string operation)
        {
            _logger.LogCritical(ex, "Postgres {Operation} error: {State}", operation, ex.SqlState);

            return DatabaseOperationException.DatabaseError(
                message: "An error occurred while executing a database operation!",
                innerException: ex
            );
        }
    }
}
