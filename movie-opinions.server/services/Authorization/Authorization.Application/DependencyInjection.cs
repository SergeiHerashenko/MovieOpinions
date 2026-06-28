using Authorization.Application.Behaviors;
using Authorization.Application.Features.Authentication.Registration;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            });

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddScoped<RegistrationFlowCoordinator>();

            return services;
        }
    }
}
