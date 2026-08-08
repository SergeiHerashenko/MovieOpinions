using Npgsql;

namespace Authorization.Infrastructure.Persistence.Context
{
    public class TransactionContext : ITransactionContext, ITransactionContextSetter
    {
        public NpgsqlConnection Connection { get; internal set; } = default!;

        public NpgsqlTransaction Transaction { get; internal set; } = default!;

        public bool IsActive =>
            Connection is not null &&
            Transaction is not null;

        public void Set(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        public void Clear()
        {
            Connection = null!;
            Transaction = null!;
        }
    }
}
