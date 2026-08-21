using Authorization.Cookie;
using Authorization.ErrorHandling;
using Authorization.MessageHandling;

namespace Authorization
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CookieSettingsOptions>(
                configuration.GetSection("CookieSettings"));

            services.AddSingleton<IErrorStatusCodeMapper, ErrorStatusCodeMapper>();
            services.AddSingleton<IErrorMessageProvider, ErrorMessageProvider>();
            services.AddScoped<ICookieProvider, CookieProvider>();

            return services;
        }
    }
}
