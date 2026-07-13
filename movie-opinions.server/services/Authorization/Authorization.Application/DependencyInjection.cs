using Authorization.Application.Behaviors;
using Authorization.Application.Common.Orchestrator;
using Authorization.Application.Common.Security.Services;
using Authorization.Application.Features.Authentication.ConfirmRegistration;
using Authorization.Application.Features.Authentication.ConfirmRegistration.Steps;
using Authorization.Application.Features.Authentication.Registration;
using Authorization.Application.Features.Authentication.SignIn;
using Authorization.Application.Features.Authentication.SignIn.Steps;
using Authorization.Application.Interfaces.Orchestrator;
using Authorization.Application.Interfaces.Security.Access;
using Authorization.Application.Interfaces.Security.Services;
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
            services.AddScoped<SignInFlowCoordinator>();

            services.AddScoped(typeof(IOrchestrator<>), typeof(Orchestrator<>));

            services.AddTransient<IOrchestratorStep<ConfirmRegistrationContext>, ProfileStep>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAccessService, AccessService>();

            services.AddScoped<IAccessStep, BlockCheck>();
            services.AddScoped<IAccessStep, DeletionCheck>();

            return services;
        }
    }
}
