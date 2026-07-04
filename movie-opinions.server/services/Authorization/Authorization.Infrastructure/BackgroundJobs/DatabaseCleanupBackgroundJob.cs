using Authorization.Application.Interfaces.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.BackgroundJobs
{
    public class DatabaseCleanupBackgroundJob : BackgroundService
    {
        private readonly ILogger<DatabaseCleanupBackgroundJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DatabaseCleanupBackgroundJob(
            ILogger<DatabaseCleanupBackgroundJob> logger, 
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("The database cleanup background service has started!");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("It's time to clean up. Cleaning up the temporary table...");

                    await CleanExpiredRegistrationsAsync(stoppingToken);

                    _logger.LogInformation("Cleaning completed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while performing background cleanup!");
                }
                // Для тесту 1 хв
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CleanExpiredRegistrationsAsync(CancellationToken ct)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IUserPendingRegistrationRepository>();

                await repo.DeleteExpiredAsync(ct);
            }
        }
    }
}
