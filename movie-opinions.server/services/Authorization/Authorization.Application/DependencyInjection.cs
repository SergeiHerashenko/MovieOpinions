using Authorization.Application.Abstractions.Orchestrator;
using Authorization.Application.Abstractions.Security.Access;
using Authorization.Application.Abstractions.Services;
using Authorization.Application.Behaviors;
using Authorization.Application.Common.Orchestrator;
using Authorization.Application.Common.Security.Services;
using Authorization.Application.Features.Registration.ConfirmRegistration;
using Authorization.Application.Features.Registration.ConfirmRegistration.Steps;
using Authorization.Application.Features.Registration.StartRegistration;
using Authorization.Application.Features.SignIn;
using Authorization.Application.Features.SignIn.Steps;
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
            services.AddTransient<IOrchestratorStep<ConfirmRegistrationContext>, ContactsStep>();
            services.AddTransient<IOrchestratorStep<ConfirmRegistrationContext>, NotificationStep>();

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped(typeof(IAccessService<>), typeof(AccessService<>));

            services.AddScoped<IAccessStep<ISignInMarker>, BlockCheck>();
            services.AddScoped<IAccessStep<ISignInMarker>, DeletionCheck>();

            return services;
        }
    }
}
