using Authorization;
using Authorization.Application;
using Authorization.Infrastructure;
using Authorization.Infrastructure.Persistence.Migrations;
using Authorization.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. НАЛАШТУВАННЯ ЛОГУВАННЯ (Serilog)
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        builder.Host.UseSerilog();

        try
        {
            Log.Information("Запуск сервісу авторизації...");

            // 2. ПЕРЕВІРКА КОНФІГУРАЦІЇ
            var userJwtKey = builder.Configuration["Authentication:UserJwt:Key"];
            var serviceJwtKey = builder.Configuration["Authentication:ServiceJwt:Key"];
            if (string.IsNullOrEmpty(userJwtKey) || userJwtKey.Length < 32)
            {
                throw new InvalidOperationException("UserJWT Key відсутній або надто короткий (мінімум 32 символи)!");
            }

            if (string.IsNullOrEmpty(serviceJwtKey) || serviceJwtKey.Length < 32)
            {
                throw new InvalidOperationException("ServiceJwt Key відсутній або надто короткий (мінімум 32 символи)!");
            }

            // 3. КОРС (CORS)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // Дозволяємо передачу Cookies
                });
            });

            // 4. РЕЄСТРАЦІЯ ШАРІВ АРХІТЕКТУРИ
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication();
            builder.Services.AddPresentation();

            // 5. КОНТРОЛЕРИ
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Щоб в JSON Enums відображалися як рядки ("Admin"), а не числа (0)
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // 6. SWAGGER / OPENAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Authorization API", Version = "v1" });

                // Налаштування Swagger для роботи з JWT Bearer
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Введіть JWT токен (без слова Bearer)"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddHttpContextAccessor();

            // 7. НАЛАШТУВАННЯ ПЕРЕДАЧІ ЗАГОЛОВКІВ (Forwarded Headers)
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto |
                    ForwardedHeaders.XForwardedHost;

                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // 8. АВТЕНТИФІКАЦІЯ ТА JWT
            builder.Services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(userJwtKey)),
                    ClockSkew = TimeSpan.Zero
                };

                // Логіка для витягування токена з Cookies (HTTP-only безпека)
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey("X-Access-Token"))
                        {
                            context.Token = context.Request.Cookies["X-Access-Token"];
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // 9. ОБРОБКА ПОМИЛОК
            builder.Services.AddExceptionHandler<AuthExceptionHandler>();
            builder.Services.AddProblemDetails();

            // 10. ГЛОБАЛЬНИЙ РЕЙТ-ЛІМІТЕР
            builder.Services.AddRateLimiter(options =>
            {
                options.AddPolicy("FixedWindowPolicy", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100, 
                            Window = TimeSpan.FromMinutes(1), 
                            QueueLimit = 0
                        }));

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            var app = builder.Build();

            // 11. ЗАПУСК МІГРАЦІЙ
            using (var scope = app.Services.CreateScope())
            {
                var migrator = scope.ServiceProvider.GetRequiredService<DatabaseMigrator>();

                await migrator.MigrateAsync();
            }

            app.UseForwardedHeaders();

            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Authorization API v1"));
            }

            app.UseHttpsRedirection();

            app.UseCors("FrontendPolicy");

            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Додаток не зміг запуститися!");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}