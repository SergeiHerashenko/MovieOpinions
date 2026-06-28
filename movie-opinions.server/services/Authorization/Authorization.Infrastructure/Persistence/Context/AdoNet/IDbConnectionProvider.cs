using Npgsql;

namespace Authorization.Infrastructure.Persistence.Context.AdoNet
{
    public interface IDbConnectionProvider
    {
        string GetConnectionString();

        Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken cancellationToken);
    }
}
