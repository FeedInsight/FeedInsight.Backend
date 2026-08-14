using Ardalis.Specification;
using FeedInsight.Domain.CustomerFeedbacks;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Development.Specifications;

public sealed class CustomerFeedbacksBySubmitterSpec
    : Specification<CustomerFeedback>
{
    public CustomerFeedbacksBySubmitterSpec(
        Guid tenantId,
        Guid submitterUserId,
        int? page,
        int? pageSize)
    {
        Query
            .AsNoTracking()
            .Where(f =>
                f.TenantId == tenantId &&
                f.SubmitterUserId == submitterUserId)
            .OrderByDescending(f => f.CreatedAt);

        if (page.HasValue && pageSize.HasValue)
        {
            Query.Skip((page.Value - 1) * pageSize.Value)
                 .Take(pageSize.Value);
        }
    }
}