using Ardalis.Specification;
using FeedInsight.Domain.CustomerFeedbacks;
using System;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Specifications;

public class CustomerFeedbacksByTenantSpec : Specification<CustomerFeedback>
{
    public CustomerFeedbacksByTenantSpec(Guid tenantId, int? page = null, int? pageSize = null)
    {
        Query
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt);

        if (page.HasValue && pageSize.HasValue)
        {
            Query
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }
    }
}
