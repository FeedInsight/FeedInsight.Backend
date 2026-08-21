using Ardalis.Specification;
using FeedInsight.Domain.DailyAnalyticsSnapshot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Analytics.Queries.Specifications
{
    public  class AnalyticsSnapshotsByDateRangeSpec
     : Specification<DailyAnalyticsSnapshot>
    {
        public AnalyticsSnapshotsByDateRangeSpec(
            Guid tenantId,
            DateOnly? from,
            DateOnly? to)
        {
            Query
                .Where(x =>
                    x.TenantId == tenantId &&
                    !x.IsDeleted &&
                    (!from.HasValue || x.SnapshotDate >= from.Value) &&
                    (!to.HasValue || x.SnapshotDate <= to.Value))
                .OrderByDescending(x => x.SnapshotDate);
        }
    }
}
