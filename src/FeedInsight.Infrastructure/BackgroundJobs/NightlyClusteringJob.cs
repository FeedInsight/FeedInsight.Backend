using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.AI.Clustering;
using FeedInsight.Application.Features.AI.TriageAgent;
using FeedInsight.Domain.ExtractedTasks;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Enums;
using FeedInsight.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeedInsight.Infrastructure.BackgroundJobs;

public class NightlyClusteringJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NightlyClusteringJob> _logger;

    public NightlyClusteringJob(IServiceProvider serviceProvider, ILogger<NightlyClusteringJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Nightly Clustering Job started (Disabled for presentation).");
        return;

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        do
        {
            try
            {
                _logger.LogInformation("Nightly Clustering Job is executing...");

                using var scope = _serviceProvider.CreateScope();
                
                var tenantRepo = scope.ServiceProvider.GetRequiredService<IRepository<Tenant>>();
                var orchestrator = scope.ServiceProvider.GetRequiredService<ITaskClusteringOrchestrator>();

                var tenants = await tenantRepo.ListAsync(stoppingToken);

                foreach (var tenant in tenants)
                {
                    await orchestrator.ProcessTenantAsync(tenant.Id, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the Nightly Clustering Job.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
        
        _logger.LogInformation("Nightly Clustering Job is stopping.");
    }
}
