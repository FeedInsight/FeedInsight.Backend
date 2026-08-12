using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.AI.Clustering;
using FeedInsight.Domain.Tenants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        _logger.LogInformation("Nightly Clustering Job started.");

        // Wait until midnight (or any specific time you prefer). For simplicity in this demo, 
        // we'll use a PeriodicTimer that triggers every 24 hours. 
        // In a real production scenario, scheduling libraries like Quartz/Hangfire are preferred.
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        do
        {
            try
            {
                _logger.LogInformation("Nightly Clustering Job is executing...");

                using var scope = _serviceProvider.CreateScope();
                
                var tenantRepo = scope.ServiceProvider.GetRequiredService<IRepository<Tenant>>();
                var clusteringService = scope.ServiceProvider.GetRequiredService<ITaskClusteringService>();

                // Fetch all active tenants
                var tenants = await tenantRepo.ListAsync(stoppingToken);

                foreach (var tenant in tenants)
                {
                    _logger.LogInformation("Processing clustering for Tenant: {TenantId}", tenant.Id);
                    
                    var clusters = await clusteringService.ClusterTenantTasksAsync(tenant.Id, stoppingToken);

                    // Once the Triage Agent is implemented, we will pass these clusters to it here.
                    // For now, we just log the output.
                    _logger.LogInformation("Clustering completed for Tenant: {TenantId}. Found {ClusterCount} clusters.", 
                        tenant.Id, clusters.Count);
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
