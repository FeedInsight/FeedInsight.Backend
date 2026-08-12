using Ardalis.Specification;
using FeedInsight.Domain.ExtractedTasks;
using FeedInsight.Domain.ExtractedTasks.Enums;

namespace FeedInsight.Application.Features.ExtractedTasks.Specifications;

public class UnassignedExtractedTasksByTenantSpec : Specification<ExtractedTask>
{
    public UnassignedExtractedTasksByTenantSpec(Guid tenantId)
    {
        Query.Where(t => t.TenantId == tenantId && t.SyncStatus == ExtractedTaskSyncStatus.Unassigned);
    }
}
