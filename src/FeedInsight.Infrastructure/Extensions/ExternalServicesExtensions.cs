using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Infrastructure.Extensions;

public static class ExternalServicesExtensions
{
    public static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddTransient<IJiraSyncService, JiraSyncService>();
        return services;
    }
}
