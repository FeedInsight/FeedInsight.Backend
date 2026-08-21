using Ardalis.Specification;
using FeedInsight.Domain.CustomerFeedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Analytics.Queries.Specifications
{
    public class FeedbacksByTenantAndDateSpec: Specification<CustomerFeedback>
    {
        public FeedbacksByTenantAndDateSpec(
       Guid tenantId,
       DateTime endDate)
        {
            Query.Where(x =>
                x.TenantId == tenantId &&
                x.CreatedAt < endDate);
        }
    }
}
