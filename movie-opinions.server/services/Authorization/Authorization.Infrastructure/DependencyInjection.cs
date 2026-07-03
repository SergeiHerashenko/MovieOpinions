using Authorization.Application.Interfaces.Communication;
using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Events;
using Authorization.Application.Interfaces.Localization;
using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Options.RateLimit;
using Authorization.Infrastructure.Context;
using Authorization.Infrastructure.Errors;
using Authorization.Infrastructure.Events;
using Authorization.Infrastructure.Http;
using Authorization.Infrastructure.Http.Options;
using Authorization.Infrastructure.Integration;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Migrations;
using Authorization.Infrastructure.Persistence.Repositories.ADO;
using Authorization.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;                                    
using Polly.Retry;
using System.Net;

namespace Authorization.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Реєстрація мігратора
            services.AddTransient<DatabaseMigrator>();

            services.AddProjectHttpClients(configuration);

            services.AddDistributedMemoryCache();

            services.Configure<RateLimitOptions>(
                configuration.GetSection("RateLimit"));

            services.Configure<NotificationServiceOptions>(
                configuration.GetSection("ExternalServices:Notification"));

            // Провайдер підключення
            services.AddSingleton<IDbConnectionProvider, ConnectAuthorizationDb>();

            // Реалізація
            services.AddScoped<IRateLimiter, RateLimiter>();
            services.AddScoped<IHasher, Hasher>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<ISendInternalRequest, SendInternalRequest>();

            services.AddSingleton<IErrorMessageProvider, ErrorMessageProvider>();
            services.AddSingleton<IClock, Clock>();

            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Репозиторії ADO
            services.AddScoped<IUserRepository, AdoUserRepository>();
            services.AddScoped<IUserPendingRegistrationRepository, AdoUserPendingRegistrationRepository>();

            // Реалізація Send
            services.AddScoped<INotificationSender, NotificationSender>();

            return services;
        }

        private static IServiceCollection AddProjectHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            string GetRequiredUrl(string sectionName)
            {
                var url = configuration[$"ExternalServices:{sectionName}:BaseUrl"];
                
                if(string.IsNullOrEmpty(url))
                {
                    throw new InvalidOperationException(
                        $"Critical configuration error: BaseUrl not found for service 'ExternalServices:{sectionName}'!");
                }

                return url;
            }

            var notificationServiceUrl = GetRequiredUrl("Notification");

            services.AddHttpClient("NotificationService", client =>
            {
                client.BaseAddress = new Uri(notificationServiceUrl);
            })
                .AddResilienceHandler("notification-retry-pipeline", builder =>
            {
                builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .HandleResult(response => response.StatusCode >= HttpStatusCode.InternalServerError),

                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(0.3),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                });
            });

            return services;
        }
    }
}
