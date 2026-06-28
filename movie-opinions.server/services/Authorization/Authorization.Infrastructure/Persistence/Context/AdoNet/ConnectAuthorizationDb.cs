using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Context.AdoNet
{
    public class ConnectAuthorizationDb : IDbConnectionProvider
    {
        private readonly IConfiguration _configuration;

        public ConnectAuthorizationDb(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection")
                   ?? throw new Exception("Connection string is missing in Secrets/Appsettings!");
        }

        public async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken cancellationToken)
        {
            var connection = new NpgsqlConnection(GetConnectionString());

            await connection.OpenAsync(cancellationToken);

            return connection;
        }
    }
}
