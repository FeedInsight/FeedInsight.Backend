using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Options;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Infrastructure.Authentication;
using FeedInsight.Infrastructure.Messaging;
using FeedInsight.Infrastructure.Persistence;
using FeedInsight.Infrastructure.Persistence.Context;
using FeedInsight.Infrastructure.Persistence.Repositories;
using FeedInsight.Infrastructure.Security;
using FeedInsight.Infrastructure.Security.Options;
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

        return services;
    }
}
