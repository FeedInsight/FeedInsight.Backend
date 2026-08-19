using Ardalis.Specification;
using FeedInsight.Domain.UserStories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Analytics.Queries.Specifications
{
    public class UserStoriesByTenantAndDateSpec : Specification<UserStory>
    {
        public UserStoriesByTenantAndDateSpec(
         Guid tenantId,
         DateTime endDate)
        {
            Query.Where(x =>
                x.TenantId == tenantId &&
                x.CreatedAt < endDate);
        }
    }
}
