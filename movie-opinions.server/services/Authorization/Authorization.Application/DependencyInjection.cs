using Authorization.Application.Behaviors;
using Authorization.Application.Features.Auth.Register.Initiate;
using Authorization.Application.Interfaces.Services;
using Authorization.Application.Services;
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

            services.AddScoped<IPendingRegistrationService, PendingRegistrationService>();
            services.AddScoped<IRegistrationNextStepResolver, RegistrationNextStepResolver>();

            return services;
        }
    }
}
