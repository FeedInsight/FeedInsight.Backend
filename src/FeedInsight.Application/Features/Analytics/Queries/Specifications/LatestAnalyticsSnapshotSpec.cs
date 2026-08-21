using Ardalis.Specification;
using FeedInsight.Domain.DailyAnalyticsSnapshot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Analytics.Queries.Specifications
{
    public  class LatestAnalyticsSnapshotSpec
     : SingleResultSpecification<DailyAnalyticsSnapshot>
    {
        public LatestAnalyticsSnapshotSpec(Guid tenantId)
        {
            Query.Where(x =>x.TenantId == tenantId &&!x.IsDeleted)
                .OrderByDescending(x => x.SnapshotDate).Take(1); 
        }
    }
}
