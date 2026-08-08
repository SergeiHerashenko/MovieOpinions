using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Repositories.Base
{
    internal abstract class CommandRepositoryBase<TRepository> : RepositoryBase<TRepository>
    {
        protected readonly ITransactionContext _transactionContext;

        public CommandRepositoryBase(
            ILogger<TRepository> logger,
            ITransactionContext transactionContext)
            : base(logger)
        {
            _transactionContext = transactionContext;
        }

        protected Task<T> ExecuteCommandAsync<T>(
            Func<NpgsqlConnection, NpgsqlTransaction, CancellationToken, Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            return ExecuteCommandCoreAsync<T>(action, cancellationToken);
        }

        protected Task ExecuteCommandAsync(
            Func<NpgsqlConnection, NpgsqlTransaction, CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            return ExecuteCommandCoreAsync<object?>(
                async (conn, transaction, ct) =>
                {
                    await action(conn, transaction, ct);
                    return null;
                },
                cancellationToken
            );
        }

        private async Task<TResult> ExecuteCommandCoreAsync<TResult>(
            Func<NpgsqlConnection, NpgsqlTransaction, CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken)
        {
            EnsureTransaction();

            try
            {
                return await action(
                    _transactionContext.Connection,
                    _transactionContext.Transaction,
                    cancellationToken);
            }
            catch (NpgsqlException ex)
            {
                throw CreateDatabaseException(ex, "command");
            }
        }

        private void EnsureTransaction()
        {
            if (!_transactionContext.IsActive)
                throw DatabaseOperationException.NoTransaction(message: "Repository operation must be executed inside UnitOfWork");
        }
    }
}
