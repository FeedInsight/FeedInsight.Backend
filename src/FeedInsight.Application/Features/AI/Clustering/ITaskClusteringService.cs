using FeedInsight.Domain.ExtractedTasks;

namespace FeedInsight.Application.Features.AI.Clustering;

public interface ITaskClusteringService
{
    /// <summary>
    /// Clusters all unassigned extracted tasks for a given tenant using DBSCAN.
    /// Noise points are discarded.
    /// </summary>
    Task<IReadOnlyList<List<ExtractedTask>>> ClusterTenantTasksAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
