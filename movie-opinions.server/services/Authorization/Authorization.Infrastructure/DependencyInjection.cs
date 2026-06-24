using Authorization.Application.Interfaces.Localization;
using Authorization.Infrastructure.Error;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IErrorMessageProvider, ErrorMessageProvider>();

            return services;
        }
    }
}
