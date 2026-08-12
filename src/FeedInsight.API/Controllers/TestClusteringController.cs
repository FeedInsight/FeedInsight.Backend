using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.AI.Clustering;
using FeedInsight.Application.Features.AI.TriageAgent;
using FeedInsight.Domain.ExtractedTasks;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Enums;
using FeedInsight.Infrastructure.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FeedInsight.API.Controllers;

[Route("api/test/clustering")]
[ApiController]
public class TestClusteringController : ControllerBase
{
    private readonly ITaskClusteringService _clusteringService;
    private readonly ITriageAgentService _triageAgent;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDatabaseService _vectorDb;
    private readonly IRepository<UserStory> _userStoryRepo;
    private readonly IRepository<ExtractedTask> _extractedTaskRepo;
    private readonly TriageAgentSettings _triageSettings;

    public TestClusteringController(
        ITaskClusteringService clusteringService,
        ITriageAgentService triageAgent,
        IEmbeddingService embeddingService,
        IVectorDatabaseService vectorDb,
        IRepository<UserStory> userStoryRepo,
        IRepository<ExtractedTask> extractedTaskRepo,
        IOptions<TriageAgentSettings> triageSettings)
    {
        _clusteringService = clusteringService;
        _triageAgent = triageAgent;
        _embeddingService = embeddingService;
        _vectorDb = vectorDb;
        _userStoryRepo = userStoryRepo;
        _extractedTaskRepo = extractedTaskRepo;
        _triageSettings = triageSettings.Value;
    }

    [HttpPost("{tenantId}")]
    public async Task<IActionResult> ClusterTenantTasks(Guid tenantId, CancellationToken cancellationToken)
    {
        var unassignedTasks = await _clusteringService.ClusterTenantTasksAsync(tenantId, cancellationToken);
        
        var response = unassignedTasks.Select((cluster, index) => new
        {
            ClusterId = index + 1,
            TaskCount = cluster.Count,
            Tasks = cluster.Select(t => new
            {
                t.Id,
                t.CustomerFeedbackId,
                t.ExtractedIntent
            })
        });

        return Ok(new
        {
            TotalClusters = unassignedTasks.Count,
            Clusters = response
        });
    }

    [HttpPost("{tenantId}/full-pipeline")]
    public async Task<IActionResult> RunFullClusteringPipeline(Guid tenantId, CancellationToken cancellationToken)
    {
        var clusters = await _clusteringService.ClusterTenantTasksAsync(tenantId, cancellationToken);
        var results = new List<object>();

        foreach (var cluster in clusters)
        {
            if (!cluster.Any()) continue;
            
            var firstTask = cluster.First();
            var categoryId = firstTask.CategoryId;

            var draftStory = await _triageAgent.ProcessClusterAsync(cluster, cancellationToken);
            
            if (string.IsNullOrWhiteSpace(draftStory.Title))
            {
                results.Add(new { Status = "Failed", Reason = "Triage Agent returned empty title" });
                continue;
            }

            var embeddingText = $"{draftStory.Title} {draftStory.AcceptanceCriteria}";
            var embedding = await _embeddingService.GenerateEmbeddingAsync(embeddingText, cancellationToken);

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

                    results.Add(new 
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

                results.Add(new 
                { 
                    Action = "Created", 
                    UserStoryId = newStory.Id, 
                    Title = newStory.Title,
                    TasksClustered = cluster.Count
                });
            }
        }

        return Ok(new
        {
            TotalClustersProcessed = clusters.Count,
            Results = results
        });
    }
}
