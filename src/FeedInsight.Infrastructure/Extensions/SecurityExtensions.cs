using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Interfaces.Security;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Infrastructure.Authentication;
using FeedInsight.Infrastructure.Security;
using FeedInsight.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Infrastructure.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IApiKeyHasher, Sha256ApiKeyHasher>();
        services.AddSingleton<IApiKeyGenerator, SecureApiKeyGenerator>();
        services.AddSingleton<IEncryptor, AesEncryptor>();
        services.AddSingleton<IJiraSignatureValidator, JiraSignatureValidator>();

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ITenantStatusChecker, TenantStatusChecker>();
        return services;
    }
}
