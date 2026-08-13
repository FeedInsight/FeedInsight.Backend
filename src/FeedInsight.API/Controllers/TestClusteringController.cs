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
    private readonly ITaskClusteringOrchestrator _orchestrator;

    public TestClusteringController(
        ITaskClusteringService clusteringService,
        ITaskClusteringOrchestrator orchestrator)
    {
        _clusteringService = clusteringService;
        _orchestrator = orchestrator;
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
        var result = await _orchestrator.ProcessTenantAsync(tenantId, cancellationToken);
        return Ok(result);
    }
}
