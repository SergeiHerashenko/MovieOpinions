using Npgsql;

namespace Authorization.Infrastructure.Persistence.Context
{
    public interface ITransactionContextSetter
    {
        void Set(
            NpgsqlConnection connection, 
            NpgsqlTransaction transaction);

        void Clear();
    }
}
