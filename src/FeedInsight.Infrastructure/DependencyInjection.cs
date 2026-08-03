using FeedInsight.Infrastructure.Extensions;
using FeedInsight.Infrastructure.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureOptions(configuration);
        services.AddPersistence(configuration);
        services.AddSecurity();
        services.AddMessaging();
        services.AddSemanticKernelAgents();
        services.AddProductAssistant();
        services.AddEmbeddings(configuration);
        services.AddVectorDatabase();

        services.AddExternalServices();

        return services;
    }
}
