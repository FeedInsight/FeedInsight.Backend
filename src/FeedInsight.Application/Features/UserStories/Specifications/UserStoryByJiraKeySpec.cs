using Ardalis.Specification;
using FeedInsight.Domain.UserStories;

namespace FeedInsight.Application.Features.UserStories.Specifications;

public class UserStoryByJiraKeySpec : Specification<UserStory>, ISingleResultSpecification<UserStory>
{
    public UserStoryByJiraKeySpec(Guid tenantId, string jiraKey)
    {
        Query.Where(us => us.TenantId == tenantId && us.JiraTicketKey == jiraKey);
    }
}