using FeedInsight.Application.Features.AI.Clustering;
using FeedInsight.Infrastructure.AI.Clustering;
using FeedInsight.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Infrastructure.Extensions.AI;

public static class ClusteringExtensions
{
    public static IServiceCollection AddClustering(this IServiceCollection services)
    {
        services.AddScoped<ITaskClusteringService, TaskClusteringService>();
        services.AddScoped<ITaskClusteringOrchestrator, TaskClusteringOrchestrator>();
        services.AddHostedService<NightlyClusteringJob>();

        return services;
    }
}
