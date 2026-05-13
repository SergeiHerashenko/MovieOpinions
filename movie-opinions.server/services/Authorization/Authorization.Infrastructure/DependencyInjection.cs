using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Errors;
using Authorization.Application.Interfaces.Repositories;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Time;
using Authorization.Application.Options.RateLimit;
using Authorization.Infrastructure.Context;
using Authorization.Infrastructure.Errors;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Repositories.ADO;
using Authorization.Infrastructure.Security;
using Authorization.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddProjectHttpClients(configuration);

            services.AddDistributedMemoryCache();

            services.Configure<RateLimitOptions>(
                configuration.GetSection("RateLimit"));

            // Провайдер підключення
            services.AddSingleton<IDbConnectionProvider, ConnectAuthorizationDb>();

            services.AddSingleton<IErrorMessageProvider, ErrorMessageProvider>();
            services.AddSingleton<ISystemClock, SystemClock>();

            // Реалізація 
            services.AddScoped<IHasher, Hasher>();
            services.AddScoped<IRateLimiter, RateLimiter>();
            services.AddScoped<IUserContext, UserContext>();

            // Репозиторії ADO
            services.AddScoped<IUserRepository, AdoUserRepository>();
            services.AddScoped<IUserPendingRegistrationRepository, AdoUserPendingRegistrationRepository>();
            services.AddScoped<IUserDeletionRepository, AdoUserDeletionRepository>();
            services.AddScoped<IUserPendingAccountChangesRepository, AdoUserPendingAccountChangesRepository>();
            services.AddScoped<IUserRefreshTokenRepository, AdoUserTokenRepository>();
            services.AddScoped<IUserRestrictionRepository, AdoUserRestrictionRepository>();

            return services;
        }

        private static IServiceCollection AddProjectHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
