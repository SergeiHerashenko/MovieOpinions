using Authorization.Cookie;
using Authorization.ErrorHandling;
using Authorization.MessageHandling;
using Authorization.ResponseHandling;
using Authorization.ResponseHandling.Abstractions;

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

            services.AddScoped<IAuthResultDispatcher, AuthResultDispatcher>();
            services.AddScoped<IAuthErrorResponseHandler, AuthErrorResponseHandler>();
            services.AddScoped<IAuthSuccessResponseHandler, AuthSuccessResponseHandler>();

            return services;
        }
    }
}
