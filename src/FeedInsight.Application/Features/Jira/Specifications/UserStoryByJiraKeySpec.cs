using Ardalis.Specification;
using FeedInsight.Domain.UserStories;

namespace FeedInsight.Application.Features.Jira.Specifications;

public sealed class UserStoryByJiraKeySpec : SingleResultSpecification<UserStory>
{
    public UserStoryByJiraKeySpec(Guid tenantId, string jiraTicketKey)
    {
        Query.Where(us => us.TenantId == tenantId && us.JiraTicketKey == jiraTicketKey);
    }
}