using Authorization.ErrorHandling;

namespace Authorization
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddSingleton<IErrorStatusCodeMapper, ErrorStatusCodeMapper>();

            return services;
        }
    }
}
