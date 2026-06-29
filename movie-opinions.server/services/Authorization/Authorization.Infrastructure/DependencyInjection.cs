using Authorization.Application.Interfaces.Communication;
using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Events;
using Authorization.Application.Interfaces.Localization;
using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Options.RateLimit;
using Authorization.Infrastructure.Context;
using Authorization.Infrastructure.Errors;
using Authorization.Infrastructure.Events;
using Authorization.Infrastructure.Integration;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Migrations;
using Authorization.Infrastructure.Persistence.Repositories.ADO;
using Authorization.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Реєстрація мігратора
            services.AddTransient<DatabaseMigrator>();

            services.AddProjectHttpClients(configuration);

            services.AddDistributedMemoryCache();

            services.Configure<RateLimitOptions>(
                configuration.GetSection("RateLimit"));

            // Провайдер підключення
            services.AddSingleton<IDbConnectionProvider, ConnectAuthorizationDb>();

            services.AddSingleton<IErrorMessageProvider, ErrorMessageProvider>();
            services.AddSingleton<IClock, Clock>();

            // Реалізація
            services.AddScoped<IRateLimiter, RateLimiter>();
            services.AddScoped<IHasher, Hasher>();
            services.AddScoped<IUserContext, UserContext>();

            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Репозиторії ADO
            services.AddScoped<IUserRepository, AdoUserRepository>();
            services.AddScoped<IUserPendingRegistrationRepository, AdoUserPendingRegistrationRepository>();

            // Реалізація Send
            services.AddScoped<INotificationSender, NotificationSender>();

            return services;
        }

        private static IServiceCollection AddProjectHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
