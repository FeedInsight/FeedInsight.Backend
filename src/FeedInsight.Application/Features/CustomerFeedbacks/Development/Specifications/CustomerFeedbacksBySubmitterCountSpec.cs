using Ardalis.Specification;
using FeedInsight.Domain.CustomerFeedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Development.Specifications
{
    public  class CustomerFeedbacksBySubmitterCountSpec : Specification<CustomerFeedback>
    {
        public CustomerFeedbacksBySubmitterCountSpec(
            Guid tenantId,
            Guid submitterUserId)
        {
            Query.Where(f =>
                f.TenantId == tenantId &&
                f.SubmitterUserId == submitterUserId);
        }
    }
}
