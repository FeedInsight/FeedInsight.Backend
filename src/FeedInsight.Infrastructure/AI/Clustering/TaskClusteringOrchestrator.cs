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
    private readonly IJiraSyncService _jiraSyncService;
    private readonly TriageAgentSettings _triageSettings;
    private readonly ILogger<TaskClusteringOrchestrator> _logger;

    public TaskClusteringOrchestrator(
        ITaskClusteringService clusteringService,
        ITriageAgentService triageAgent,
        IEmbeddingService embeddingService,
        IVectorDatabaseService vectorDb,
        IRepository<UserStory> userStoryRepo,
        IRepository<ExtractedTask> extractedTaskRepo,
        IJiraSyncService jiraSyncService,
        IOptions<TriageAgentSettings> triageSettings,
        ILogger<TaskClusteringOrchestrator> logger)
    {
        _clusteringService = clusteringService;
        _triageAgent = triageAgent;
        _embeddingService = embeddingService;
        _vectorDb = vectorDb;
        _userStoryRepo = userStoryRepo;
        _extractedTaskRepo = extractedTaskRepo;
        _jiraSyncService = jiraSyncService;
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
                limit: _triageSettings.CandidateRetrievalLimit, 
                filter: filter, 
                cancellationToken: cancellationToken);

            var candidateStories = searchResults
                .Where(r => r.Score >= _triageSettings.DeduplicationSimilarityThreshold)
                .ToList();

            var llmDecision = await _triageAgent.DetermineDeduplicationAsync(draftStory, candidateStories, cancellationToken);

            if (llmDecision.IsDuplicate && llmDecision.DuplicateOfStoryId.HasValue)
            {
                var targetId = llmDecision.DuplicateOfStoryId.Value;
                _logger.LogInformation("LLM Duplicate User Story found! Merging cluster into existing story {UserStoryId}. Reasoning: {Reasoning}", targetId, llmDecision.Reasoning);
                
                var existingStory = await _userStoryRepo.GetByIdAsync(targetId, cancellationToken);
                if (existingStory != null)
                {
                    existingStory.IncreaseUrgency(cluster.Count);
                    await _userStoryRepo.UpdateAsync(existingStory, cancellationToken);
                    
                    foreach (var task in cluster)
                    {
                        task.AssignToStory(existingStory.Id);
                        await _extractedTaskRepo.UpdateAsync(task, cancellationToken);
                    }

                    if (existingStory.Status == UserStoryStatus.Synced)
                    {
                        _logger.LogInformation("Story {UserStoryId} is synced. Updating Jira with new urgency.", existingStory.Id);
                        await _jiraSyncService.PushStoryToJiraAsync(tenantId, existingStory, cancellationToken);
                    }

                    result.Results.Add(new ClusterProcessResult 
                    { 
                        Action = "Merged", 
                        UserStoryId = existingStory.Id, 
                        Title = existingStory.Title,
                        Score = candidateStories.FirstOrDefault(c => c.PointId == targetId)?.Score ?? 0,
                        TasksClustered = cluster.Count
                    });
                }
            }
            else
            {
                _logger.LogInformation("No duplicate found by LLM. Creating a new Draft User Story. Reasoning: {Reasoning}", llmDecision.Reasoning);
                
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
