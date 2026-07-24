using FeedInsight.Application.Features.Feeds;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Infrastructure.Messaging;
using FeedInsight.Infrastructure.Persistence;
using FeedInsight.Infrastructure.Persistence.Context;
using FeedInsight.Infrastructure.Persistence.Repositories;
using FeedInsight.Infrastructure.Security;
using FeedInsight.Infrastructure.Security.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind Options (Options Pattern)
        services.Configure<SecuritySettings>(configuration.GetSection(SecuritySettings.SectionName));

        // register concrete mediator implementation
        services.AddSingleton<IMediator, Mediator>();

        services.AddDbContext<FeedInsightDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFeedRepository, FeedRepository>();

        // Security & Cryptography
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IApiKeyHasher, Sha256ApiKeyHasher>();
        services.AddSingleton<IApiKeyGenerator, SecureApiKeyGenerator>();
        services.AddSingleton<IEncryptor, AesEncryptor>();

        return services;
    }
}
