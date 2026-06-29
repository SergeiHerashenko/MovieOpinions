using Authorization.Application.Interfaces.Security;
using Authorization.Infrastructure.Exceptions;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Npgsql;

namespace Authorization.Infrastructure.Persistence.Migrations
{
    public class DatabaseMigrator
    {
        private readonly IDbConnectionProvider _dbConnectionProvider;
        private readonly IClock _clock;

        public DatabaseMigrator(
            IDbConnectionProvider dbConnectionProvider,
            IClock clock)
        {
            _dbConnectionProvider = dbConnectionProvider;
            _clock = clock;
        }

        public async Task MigrateAsync()
        {
            using var connection = await _dbConnectionProvider.GetOpenConnectionAsync(CancellationToken.None);

            // Гарантуємо наявність таблиці історії міграцій
            await EnsureHistoryTableExistsAsync(connection);

            // Зчитуємо вбудовані файли міграцій (.psql)
            var assembly = typeof(DatabaseMigrator).Assembly;
            var availableMigrations = assembly.GetManifestResourceNames()
                .Where(name => name.EndsWith(".psql"))
                .OrderBy(name => name)
                .ToList();

            if (!availableMigrations.Any()) 
                return;

            var appliedMigrations = await GetAppliedMigrationsAsync(connection);

            foreach (var migrationName in availableMigrations)
            {
                // Витягуємо чисте ім'я файлу
                string resourcePrefix = $"{typeof(DatabaseMigrator).Namespace}.";
                string shortName = migrationName.Replace(resourcePrefix, "");

                if (appliedMigrations.Contains(shortName))
                    continue;

                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // Читаємо текст SQL з ресурсів
                    using var stream = assembly.GetManifestResourceStream(migrationName);
                    using var reader = new StreamReader(
                        stream 
                        ?? throw DatabaseOperationException.NotFoundFile(
                            $"Resource {migrationName} not found!",
                            new Dictionary<string, object>
                            {
                                ["migrationName"] = shortName,
                                ["FullResourcePath"] = migrationName,
                                ["AssemblyName"] = assembly.GetName().Name ?? "Unknown Assembly"
                            }
                        )
                    );
                    string sqlScript = await reader.ReadToEndAsync();

                    // Виконуємо сам скрипт міграції
                    using var cmd = new NpgsqlCommand(sqlScript, connection, transaction);
                    await cmd.ExecuteNonQueryAsync();

                    // Фіксуємо міграцію
                    string logSql = @"
                                    INSERT INTO 
                                        migration_history (version_name, applied_at) 
                                    VALUES 
                                        (@Name, @Now);";

                    using var logCmd = new NpgsqlCommand(logSql, connection, transaction);
                    logCmd.Parameters.AddWithValue("@Name", shortName);
                    logCmd.Parameters.AddWithValue("@Now", _clock.UtcNow);
                    await logCmd.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    throw DatabaseOperationException.ExceptionMigrate(
                        $"Failed to apply migration {shortName}. Transaction rolled back.",
                        ex
                    );
                }
            }
        }

        private async Task EnsureHistoryTableExistsAsync(NpgsqlConnection connection)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS migration_history ( 
                    id SERIAL PRIMARY KEY, 
                    version_name VARCHAR(255) NOT NULL UNIQUE, 
                    applied_at TIMESTAMPTZ NOT NULL
                );";

            using var cmd = new NpgsqlCommand(sql, connection);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<HashSet<string>> GetAppliedMigrationsAsync(NpgsqlConnection connection)
        {
            var result = new HashSet<string>();

            const string sql = "SELECT version_name FROM migration_history;";

            using var cmd = new NpgsqlCommand(sql, connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(reader.GetString(0));
            }

            return result;
        }
    }
}
