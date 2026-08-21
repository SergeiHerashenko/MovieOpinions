using Authorization.Application.Abstractions.AggregateChanges;
using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Communication;
using Authorization.Application.Abstractions.Events;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.RateLimiter;
using Authorization.Application.Abstractions.Security.Hashers;
using Authorization.Application.Abstractions.Security.JWT;
using Authorization.Application.Abstractions.UserContext;
using Authorization.Infrastructure.AggregateChanges;
using Authorization.Infrastructure.Communication;
using Authorization.Infrastructure.Communication.Options;
using Authorization.Infrastructure.Context;
using Authorization.Infrastructure.Events;
using Authorization.Infrastructure.Http;
using Authorization.Infrastructure.Limiter;
using Authorization.Infrastructure.Limiter.Options;
using Authorization.Infrastructure.Persistence.Context;
using Authorization.Infrastructure.Persistence.Context.AdoNet;
using Authorization.Infrastructure.Persistence.Migrations;
using Authorization.Infrastructure.Persistence.Repositories.UserDeletedRepository.Ado;
using Authorization.Infrastructure.Persistence.Repositories.UserPendingActionRepository.Ado;
using Authorization.Infrastructure.Persistence.Repositories.UserPendingRegistrationRepository.Ado;
using Authorization.Infrastructure.Persistence.Repositories.UserRefreshTokenRepository.Ado;
using Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado;
using Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado.Loading;
using Authorization.Infrastructure.Persistence.Repositories.UserRestrictionRepository.Ado;
using Authorization.Infrastructure.Persistence.Repositories.UserRestrictionSessionRepository.Ado;
using Authorization.Infrastructure.Persistence.UnitOfWorks;
using Authorization.Infrastructure.Security.Hashers;
using Authorization.Infrastructure.Security.JWT;
using Authorization.Infrastructure.Security.JWT.Interfaces;
using Authorization.Infrastructure.Security.JWT.Options;
using Authorization.Infrastructure.SystemClock;
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
            services.AddDistributedMemoryCache();

            services.AddProjectHttpClients(configuration);

            // Реєстрація мігратора
            services.AddTransient<DatabaseMigrator>();

            // Провайдер підключення
            services.AddScoped<IDbConnectionProvider, ConnectAuthorizationDb>();

            services.Configure<RateLimitOptions>(
                configuration.GetSection(RateLimitOptions.SectionName));

            services.Configure<ServiceJwtProviderOptions>(
                configuration.GetSection(ServiceJwtProviderOptions.SectionName));

            services.Configure<UserJwtProviderOptions>(
                configuration.GetSection(UserJwtProviderOptions.SectionName)); 

            services.Configure<ServiceIdentityOptions>(
                configuration.GetSection(ServiceIdentityOptions.SectionName));

            services.Configure<NotificationServiceOptions>(
                configuration.GetSection($"{ExternalServicesSection}:{NotificationServiceOptions.SectionName}"));

            services.Configure<VerificationServiceOptions>(
                configuration.GetSection($"{ExternalServicesSection}:{VerificationServiceOptions.SectionName}"));

            services.Configure<ContactsServiceOptions>(
                configuration.GetSection($"{ExternalServicesSection}:{ContactsServiceOptions.SectionName}"));

            services.Configure<ProfileServiceOptions>(
                configuration.GetSection($"{ExternalServicesSection}:{ProfileServiceOptions.SectionName}"));

            services.AddScoped<TransactionContext>();
            services.AddScoped<ITransactionContext>(sp => sp.GetRequiredService<TransactionContext>());
            services.AddScoped<ITransactionContextSetter>(sp => sp.GetRequiredService<TransactionContext>());

            // Реалізація
            services.AddSingleton<IClock, Clock>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IRateLimiter, RateLimiter>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<IAggregateChangesDispatcher, AggregateChangesDispatcher>();
            services.AddScoped<ISendInternalRequest, SendInternalRequest>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IServiceJwtProvider, ServiceJwtProvider>();
            services.AddScoped<IUserJwtProvider, UserJwtProvider>();

            // Реалізація Send
            services.AddScoped<INotificationSender, NotificationSender>();
            services.AddScoped<IVerificationSender, VerificationSender>();
            services.AddScoped<IContactsSender, ContactsSender>();
            services.AddScoped<IProfileSender, ProfileSender>();

            // Репозиторії ADO
            services.AddScoped<AdoUserAggregateLoader>();

            services.AddScoped<AdoUserQueryRepository>();
            services.AddScoped<AdoUserCommandRepository>();
            services.AddScoped<IUserRepository, AdoUserRepository>();

            services.AddScoped<AdoUserRefreshTokenQueryRepository>();
            services.AddScoped<AdoUserRefreshTokenCommandRepository>();
            services.AddScoped<IUserRefreshTokenRepository, AdoUserRefreshTokenRepository>();

            services.AddScoped<AdoUserPendingRegistrationQueryRepository>();
            services.AddScoped<AdoUserPendingRegistrationCommandRepository>();
            services.AddScoped<IUserPendingRegistrationRepository, AdoUserPendingRegistrationRepository>();

            services.AddScoped<AdoUserRestrictionQueryRepository>();
            services.AddScoped<AdoUserRestrictionCommandRepository>();
            services.AddScoped<IUserRestrictionRepository, AdoUserRestrictionRepository>();

            services.AddScoped<AdoUserRestrictionSessionQueryRepository>();
            services.AddScoped<AdoUserRestrictionSessionCommandRepository>();
            services.AddScoped<IUserRestrictionSessionRepository, AdoUserRestrictionSessionRepository>();

            services.AddScoped<AdoUserDeletedQueryRepository>();
            services.AddScoped<AdoUserDeletedCommandRepository>();
            services.AddScoped<IUserDeletedRepository, AdoUserDeletedRepository>();

            services.AddScoped<AdoUserPendingActionQueryRepository>();
            services.AddScoped<AdoUserPendingActionCommandRepository>();
            services.AddScoped<IUserPendingActionRepository, AdoUserPendingActionRepository>();

            return services;
        }

        private static IServiceCollection AddProjectHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            string GetRequiredUrl(string sectionName)
            {
                var url = configuration[$"{ExternalServicesSection}:{sectionName}:{BaseUrlKey}"];

                if (string.IsNullOrEmpty(url))
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
            RegisterService(ProfileServiceOptions.SectionName, ProfilePipeline);

            return services;
        }
    }
}
