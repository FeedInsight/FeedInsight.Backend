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
        _logger.LogInformation("Nightly Clustering Job started.");

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        do
        {
            try
            {
                _logger.LogInformation("Nightly Clustering Job is executing...");

                using var scope = _serviceProvider.CreateScope();
                
                var tenantRepo = scope.ServiceProvider.GetRequiredService<IRepository<Tenant>>();
                var clusteringService = scope.ServiceProvider.GetRequiredService<ITaskClusteringService>();
                var triageAgent = scope.ServiceProvider.GetRequiredService<ITriageAgentService>();
                var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
                var vectorDb = scope.ServiceProvider.GetRequiredService<IVectorDatabaseService>();
                var userStoryRepo = scope.ServiceProvider.GetRequiredService<IRepository<UserStory>>();
                var extractedTaskRepo = scope.ServiceProvider.GetRequiredService<IRepository<ExtractedTask>>();
                var triageSettings = scope.ServiceProvider.GetRequiredService<IOptions<TriageAgentSettings>>().Value;

                var tenants = await tenantRepo.ListAsync(stoppingToken);

                foreach (var tenant in tenants)
                {
                    _logger.LogInformation("Processing clustering for Tenant: {TenantId}", tenant.Id);
                    
                    var clusters = await clusteringService.ClusterTenantTasksAsync(tenant.Id, stoppingToken);

                    _logger.LogInformation("Clustering completed for Tenant: {TenantId}. Found {ClusterCount} clusters.", 
                        tenant.Id, clusters.Count);

                    foreach (var cluster in clusters)
                    {
                        if (!cluster.Any()) continue;
                        
                        var firstTask = cluster.First();
                        var categoryId = firstTask.CategoryId;

                        // 1. Synthesize Draft User Story using Triage Agent
                        var draftStory = await triageAgent.ProcessClusterAsync(cluster, stoppingToken);
                        
                        if (string.IsNullOrWhiteSpace(draftStory.Title))
                        {
                            _logger.LogWarning("Triage agent failed to generate a story title for cluster. Skipping.");
                            continue;
                        }

                        // 2. Embed the synthesized story
                        var embeddingText = $"{draftStory.Title} {draftStory.AcceptanceCriteria}";
                        var embedding = await embeddingService.GenerateEmbeddingAsync(embeddingText, stoppingToken);

                        // 3. Search Qdrant for semantic duplicates
                        var filter = new MetadataFilter
                        {
                            MustMatch = new Dictionary<string, object>
                            {
                                { "TenantId", tenant.Id.ToString() }
                            }
                        };

                        var searchResults = await vectorDb.SearchAsync<UserStoryPayload>(
                            "user_stories", 
                            embedding, 
                            limit: 1, 
                            filter: filter, 
                            cancellationToken: stoppingToken);

                        var topResult = searchResults.FirstOrDefault();

                        if (topResult != null && topResult.Score >= triageSettings.DeduplicationSimilarityThreshold)
                        {
                            _logger.LogInformation("Duplicate User Story found! Merging cluster into existing story {UserStoryId}", topResult.PointId);
                            
                            var existingStory = await userStoryRepo.GetByIdAsync(topResult.PointId, stoppingToken);
                            if (existingStory != null)
                            {
                                existingStory.IncreaseUrgency(cluster.Count);
                                await userStoryRepo.UpdateAsync(existingStory, stoppingToken);
                                
                                foreach (var task in cluster)
                                {
                                    task.AssignToStory(existingStory.Id);
                                    await extractedTaskRepo.UpdateAsync(task, stoppingToken);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInformation("No duplicate found. Creating a new Draft User Story.");
                            
                            var newStory = new UserStory(
                                tenantId: tenant.Id,
                                categoryId: categoryId,
                                source: UserStorySource.FeedInsight,
                                title: draftStory.Title,
                                acceptanceCriteria: draftStory.AcceptanceCriteria);
                                
                            newStory.IncreaseUrgency(cluster.Count);
                            await userStoryRepo.AddAsync(newStory, stoppingToken);

                            foreach (var task in cluster)
                            {
                                task.AssignToStory(newStory.Id);
                                await extractedTaskRepo.UpdateAsync(task, stoppingToken);
                            }
                        }
                    }
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
