using Authorization.Application.Interfaces.Errors;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Security.Configurations;
using Authorization.Infrastructure.Errors;
using Authorization.Infrastructure.Security;
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

            services.AddScoped<IRateLimiter, RateLimiter>();
            services.AddScoped<IErrorMessageProvider, ErrorMessageProvider>();

            return services;
        }

        private static IServiceCollection AddProjectHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
