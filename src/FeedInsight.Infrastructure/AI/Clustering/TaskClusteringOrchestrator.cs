using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.AI.Clustering;
using FeedInsight.Application.Features.AI.Clustering.Models;
using FeedInsight.Application.Features.AI.TriageAgent;
using FeedInsight.Domain.ExtractedTasks;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Enums;
using FeedInsight.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeedInsight.Infrastructure.AI.Clustering;

public class TaskClusteringOrchestrator : ITaskClusteringOrchestrator
{
    private readonly ITaskClusteringService _clusteringService;
    private readonly ITriageAgentService _triageAgent;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDatabaseService _vectorDb;
    private readonly IRepository<UserStory> _userStoryRepo;
    private readonly IRepository<ExtractedTask> _extractedTaskRepo;
    private readonly TriageAgentSettings _triageSettings;
    private readonly ILogger<TaskClusteringOrchestrator> _logger;

    public TaskClusteringOrchestrator(
        ITaskClusteringService clusteringService,
        ITriageAgentService triageAgent,
        IEmbeddingService embeddingService,
        IVectorDatabaseService vectorDb,
        IRepository<UserStory> userStoryRepo,
        IRepository<ExtractedTask> extractedTaskRepo,
        IOptions<TriageAgentSettings> triageSettings,
        ILogger<TaskClusteringOrchestrator> logger)
    {
        _clusteringService = clusteringService;
        _triageAgent = triageAgent;
        _embeddingService = embeddingService;
        _vectorDb = vectorDb;
        _userStoryRepo = userStoryRepo;
        _extractedTaskRepo = extractedTaskRepo;
        _triageSettings = triageSettings.Value;
        _logger = logger;
    }

    public async Task<TenantClusteringResult> ProcessTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing clustering for Tenant: {TenantId}", tenantId);
        
        var clusters = await _clusteringService.ClusterTenantTasksAsync(tenantId, cancellationToken);
        var result = new TenantClusteringResult
        {
            TotalClustersProcessed = clusters.Count
        };

        _logger.LogInformation("Clustering completed for Tenant: {TenantId}. Found {ClusterCount} clusters.", 
            tenantId, clusters.Count);

        foreach (var cluster in clusters)
        {
            if (!cluster.Any()) continue;
            
            var firstTask = cluster.First();
            var categoryId = firstTask.CategoryId;

            // 1. Synthesize Draft User Story using Triage Agent
            var draftStory = await _triageAgent.ProcessClusterAsync(cluster, cancellationToken);
            
            if (string.IsNullOrWhiteSpace(draftStory.Title))
            {
                _logger.LogWarning("Triage agent failed to generate a story title for cluster. Skipping.");
                result.Results.Add(new ClusterProcessResult 
                { 
                    Action = "Failed", 
                    Reason = "Triage Agent returned empty title",
                    TasksClustered = cluster.Count 
                });
                continue;
            }

            // 2. Embed the synthesized story
            var embeddingText = $"{draftStory.Title} {draftStory.AcceptanceCriteria}";
            var embedding = await _embeddingService.GenerateEmbeddingAsync(embeddingText, cancellationToken);

            // 3. Search Qdrant for semantic duplicates
            var filter = new MetadataFilter
            {
                MustMatch = new Dictionary<string, object>
                {
                    { "TenantId", tenantId.ToString() }
                }
            };

            var searchResults = await _vectorDb.SearchAsync<UserStoryPayload>(
                "user_stories", 
                embedding, 
                limit: 1, 
                filter: filter, 
                cancellationToken: cancellationToken);

            var topResult = searchResults.FirstOrDefault();

            if (topResult != null && topResult.Score >= _triageSettings.DeduplicationSimilarityThreshold)
            {
                _logger.LogInformation("Duplicate User Story found! Merging cluster into existing story {UserStoryId}", topResult.PointId);
                
                var existingStory = await _userStoryRepo.GetByIdAsync(topResult.PointId, cancellationToken);
                if (existingStory != null)
                {
                    existingStory.IncreaseUrgency(cluster.Count);
                    await _userStoryRepo.UpdateAsync(existingStory, cancellationToken);
                    
                    foreach (var task in cluster)
                    {
                        task.AssignToStory(existingStory.Id);
                        await _extractedTaskRepo.UpdateAsync(task, cancellationToken);
                    }

                    result.Results.Add(new ClusterProcessResult 
                    { 
                        Action = "Merged", 
                        UserStoryId = existingStory.Id, 
                        Title = existingStory.Title,
                        Score = topResult.Score,
                        TasksClustered = cluster.Count
                    });
                }
            }
            else
            {
                _logger.LogInformation("No duplicate found. Creating a new Draft User Story.");
                
                var newStory = new UserStory(
                    tenantId: tenantId,
                    categoryId: categoryId,
                    source: UserStorySource.FeedInsight,
                    title: draftStory.Title,
                    acceptanceCriteria: draftStory.AcceptanceCriteria);
                    
                newStory.IncreaseUrgency(cluster.Count);
                await _userStoryRepo.AddAsync(newStory, cancellationToken);

                foreach (var task in cluster)
                {
                    task.AssignToStory(newStory.Id);
                    await _extractedTaskRepo.UpdateAsync(task, cancellationToken);
                }

                result.Results.Add(new ClusterProcessResult 
                { 
                    Action = "Created", 
                    UserStoryId = newStory.Id, 
                    Title = newStory.Title,
                    TasksClustered = cluster.Count
                });
            }
        }

        return result;
    }
}
