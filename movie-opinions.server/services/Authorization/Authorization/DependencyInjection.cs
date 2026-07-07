using Authorization.Cookie;
using Authorization.ErrorHandling;

namespace Authorization
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddSingleton<IErrorStatusCodeMapper, ErrorStatusCodeMapper>();
            services.AddScoped<ICookieProvider, CookieProvider>();

            return services;
        }
    }
}
