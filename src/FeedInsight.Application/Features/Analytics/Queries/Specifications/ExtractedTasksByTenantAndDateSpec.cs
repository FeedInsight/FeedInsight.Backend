using Ardalis.Specification;
using FeedInsight.Domain.ExtractedTasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Analytics.Queries.Specifications
{
    public class ExtractedTasksByTenantAndDateSpec : Specification<ExtractedTask>
    {
        public ExtractedTasksByTenantAndDateSpec(
            Guid tenantId,
            DateTime startDate,
            DateTime endDate)
        {
            Query
                .Where(x =>
                    x.TenantId == tenantId &&
                    x.CreatedAt >= startDate &&
                    x.CreatedAt < endDate);
        }
    }
}
