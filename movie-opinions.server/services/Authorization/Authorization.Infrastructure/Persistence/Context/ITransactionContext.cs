using Npgsql;

namespace Authorization.Infrastructure.Persistence.Context
{
    public interface ITransactionContext
    {
        NpgsqlConnection Connection { get; }

        NpgsqlTransaction Transaction { get; }

        bool IsActive { get; }
    }
}
