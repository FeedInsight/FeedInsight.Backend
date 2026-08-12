using FeedInsight.Application.Features.AI.Clustering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.API.Controllers;

[Route("api/test/clustering")]
[ApiController]
public class TestClusteringController : ControllerBase
{
    private readonly ITaskClusteringService _clusteringService;

    public TestClusteringController(ITaskClusteringService clusteringService)
    {
        _clusteringService = clusteringService;
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
}
