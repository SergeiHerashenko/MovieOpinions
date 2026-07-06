using Authorization.Application.Interfaces.Communication;
using Authorization.Application.Interfaces.Context;
using Authorization.Application.Interfaces.Events;
using Authorization.Application.Interfaces.Localization;
using Authorization.Application.Interfaces.Persistence;
using Authorization.Application.Interfaces.Security;
using Authorization.Application.Interfaces.Security.JWT;
using Authorization.Application.Options.RateLimit;
using Authorization.Infrastructure.BackgroundJobs;
using Authorization.Infrastructure.Context;
using Authorization.Infrastructure.Errors;
using Authorization.Infrastructure.Events;
using Authorization.Infrastructure.Http;
using Authorization.Infrastructure.Integration;
using Authorization.Infrastructure.Integration.Options;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Migrations;
using Authorization.Infrastructure.Persistence.Repositories.ADO;
using Authorization.Infrastructure.Security;
using Authorization.Infrastructure.Security.JWT;
using Authorization.Infrastructure.Security.JWT.Interfaces;
using Authorization.Infrastructure.Security.JWT.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using System.Net;

namespace Authorization.Infrastructure
{
    public static class DependencyInjection
    {
        private const string ExternalServicesSection = "ExternalServices";
        private const string BaseUrlKey = "BaseUrl";
        private const string NotificationPipeline = "notification-retry-pipeline";
        private const string VerificationPipeline = "verification-retry-pipeline";
        private const string ContactsPipeline = "contacts-retry-pipeline";
        private const string ProfilePipeline = "profile-retry-pipeline";

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Реєстрація мігратора
            services.AddTransient<DatabaseMigrator>();

            services.AddProjectHttpClients(configuration);

            services.AddDistributedMemoryCache();

            services.Configure<RateLimitOptions>(
                configuration.GetSection(RateLimitOptions.SectionName));

            services.Configure<UserJwtProviderOptions>(
                configuration.GetSection(UserJwtProviderOptions.SectionName));

            services.Configure<NotificationServiceOptions>(
                configuration.GetSection($"{ExternalServicesSection}:{ProfileServiceOption.SectionName}"));

            services.Configure<VerificationServiceOptions>(
                configuration.GetSection($"{ExternalServicesSection}:{VerificationServiceOptions.SectionName}"));

            services.Configure<ContactsServiceOptions>(
                configuration.GetSection($"{ExternalServicesSection}:{ContactsServiceOptions.SectionName}"));

            services.Configure<ProfileServiceOption>(
                configuration.GetSection($"{ExternalServicesSection}:{ProfileServiceOption.SectionName}"));

            services.Configure<ServiceJwtProviderOptions>(
                configuration.GetSection(ServiceJwtProviderOptions.SectionName));

            services.Configure<ServiceIdentityOptions>(
                configuration.GetSection(ServiceIdentityOptions.SectionName));

            // Провайдер підключення
            services.AddSingleton<IDbConnectionProvider, ConnectAuthorizationDb>();

            // Реалізація
            services.AddScoped<IRateLimiter, RateLimiter>();
            services.AddScoped<IHasher, Hasher>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<ISendInternalRequest, SendInternalRequest>();
            services.AddScoped<IServiceJwtProvider, ServiceJwtProvider>();

            services.AddSingleton<IErrorMessageProvider, ErrorMessageProvider>();
            services.AddSingleton<IClock, Clock>();
            services.AddScoped<IUserJwtProvider, UserJwtProvider>();

            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Репозиторії ADO
            services.AddScoped<IUserRepository, AdoUserRepository>();
            services.AddScoped<IUserPendingRegistrationRepository, AdoUserPendingRegistrationRepository>();
            services.AddScoped<IUserRefreshTokenRepository, AdoUserRefreshTokenRepository>();

            // Реалізація Send
            services.AddScoped<INotificationSender, NotificationSender>();
            services.AddScoped<IVerificationSender, VerificationSender>();
            services.AddScoped<IContactsSender, ContactsSender>();
            services.AddScoped<IProfileSender, ProfileSender>();

            services.AddHostedService<DatabaseCleanupBackgroundJob>();

            return services;
        }

        private static IServiceCollection AddProjectHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            string GetRequiredUrl(string sectionName)
            {
                var url = configuration[$"{ExternalServicesSection}:{sectionName}:{BaseUrlKey}"];
                
                if(string.IsNullOrEmpty(url))
                {
                    throw new InvalidOperationException(
                        $"Critical configuration error: BaseUrl not found for service '{ExternalServicesSection}:{sectionName}'!");
                }

                return url;
            }

            string GetClientName(string sectionName)
            {
                var client = configuration[$"{ExternalServicesSection}:{sectionName}:ClientName"];

                if (string.IsNullOrEmpty(client))
                {
                    throw new InvalidOperationException(
                        $"Critical configuration error: ClientName not found for service '{ExternalServicesSection}:{sectionName}'!");
                }

                return client;
            }

            void RegisterService(string sectionKey, string pipleName, double timeDelayForSecond = 0.1)
            {
                var clientName = GetClientName(sectionKey);
                var baseUrl = GetRequiredUrl(sectionKey);

                services.AddHttpClient(clientName, client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                })
                    .AddResilienceHandler(pipleName, builder =>
                    {
                        builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                        {
                            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                           .Handle<HttpRequestException>()
                           .HandleResult(response => response.StatusCode >= HttpStatusCode.InternalServerError),

                            MaxRetryAttempts = 3,
                            Delay = TimeSpan.FromSeconds(timeDelayForSecond),
                            BackoffType = DelayBackoffType.Exponential,
                            UseJitter = true
                        });
                    });
            }

            // --- Service ---
            RegisterService(NotificationServiceOptions.SectionName, NotificationPipeline);
            RegisterService(VerificationServiceOptions.SectionName, VerificationPipeline);
            RegisterService(ContactsServiceOptions.SectionName, ContactsPipeline);
            RegisterService(ProfileServiceOption.SectionName, ProfilePipeline);
            
            return services;
        }
    }
}
