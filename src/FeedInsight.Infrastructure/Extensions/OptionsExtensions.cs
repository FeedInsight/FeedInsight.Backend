using FeedInsight.Application.Common.Options;
using FeedInsight.Infrastructure.Options;
using FeedInsight.Infrastructure.Security.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Infrastructure.Extensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddInfrastructureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SecuritySettings>(configuration.GetSection(SecuritySettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<QdrantSettings>(configuration.GetSection(QdrantSettings.SectionName));
        services.Configure<HuggingFaceSettings>(configuration.GetSection(HuggingFaceSettings.SectionName));
        return services;
    }
}
