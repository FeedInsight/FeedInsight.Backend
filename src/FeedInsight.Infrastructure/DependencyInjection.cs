using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Options;
using FeedInsight.Application.Features.AI.RouterAgent;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Infrastructure.AI.RouterAgent;
using FeedInsight.Infrastructure.Authentication;
using FeedInsight.Infrastructure.BackgroundJobs;
using FeedInsight.Infrastructure.Messaging;
using FeedInsight.Infrastructure.Persistence;
using FeedInsight.Infrastructure.Persistence.Context;
using FeedInsight.Infrastructure.Persistence.Repositories;
using FeedInsight.Infrastructure.Security;
using FeedInsight.Infrastructure.Security.Options;
using FeedInsight.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace FeedInsight.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind Options (Options Pattern)
        services.Configure<SecuritySettings>(configuration.GetSection(SecuritySettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // register concrete mediator implementation
        services.AddScoped<IMediator, Mediator>();

        services.AddDbContext<FeedInsightDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Security & Cryptography
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IApiKeyHasher, Sha256ApiKeyHasher>();
        services.AddSingleton<IApiKeyGenerator, SecureApiKeyGenerator>();
        services.AddSingleton<IEncryptor, AesEncryptor>();

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ITenantStatusChecker, TenantStatusChecker>();

        // 5. AI & Semantic Kernel (ADDED)
        services.AddScoped<IRouterAgentService, RouterAgentService>();

        services.AddTransient<Kernel>(sp =>
        {
            var builder = Kernel.CreateBuilder();

            // Pulling credentials from appsettings.json
            var apiKey = configuration["OpenAI:ApiKey"];
            var modelId = configuration["OpenAI:ModelId"] ?? "gpt-4o-mini"; // Defaulting to 4o-mini for cost efficiency

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("OpenAI API Key is missing from configuration.");
            }

            builder.AddOpenAIChatCompletion(modelId, apiKey);

            return builder.Build();
        });

        // registe the background services (hosted services are singletons
        services.AddHostedService<RouterBackgroundService>();

        return services;
    }
}
