using Authorization.Application.Abstractions.Persistence;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context;
using Authorization.Infrastructure.Persistence.Context.AdoNet;

namespace Authorization.Infrastructure.Persistence.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;
        private readonly ITransactionContextSetter _transactionContextSetter;
        private readonly ITransactionContext _transactionContext;

        public UnitOfWork(
            IDbConnectionProvider dbConnectionProvider,
            ITransactionContextSetter transactionContextSetter,
            ITransactionContext transactionContext)
        {
            _dbConnectionProvider = dbConnectionProvider;
            _transactionContextSetter = transactionContextSetter;
            _transactionContext = transactionContext;
        }

        public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            return ExecuteAsync<object?>(
                async ct =>
                {
                    await action(ct);
                    return null;
                },
                cancellationToken
            );
        }

        public Task<TResult> ExecuteAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action, 
            CancellationToken cancellationToken = default)
        {
            return ExecuteTransactionAsync(action, cancellationToken);
        }

        private async Task<TResult> ExecuteTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action, 
            CancellationToken cancellationToken = default)
        {
            EnsureNoTransaction();

            await using var connection = await _dbConnectionProvider.GetOpenConnectionAsync(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            _transactionContextSetter.Set(connection, transaction);

            try
            {
                var result = await action(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                _transactionContextSetter.Clear();
            }
        }

        private void EnsureNoTransaction()
        {
            if (_transactionContext.IsActive)
                throw DatabaseOperationException.NestedTransaction("Nested UnitOfWork is not supported!");
        }
    }
}
