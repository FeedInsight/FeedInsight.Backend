using FeedInsight.Application.Features.AI.Clustering.Models;

namespace FeedInsight.Application.Features.AI.Clustering;

public interface ITaskClusteringOrchestrator
{
    /// <summary>
    /// Processes clustering, triaging, and user story generation for a specific tenant.
    /// </summary>
    Task<TenantClusteringResult> ProcessTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
