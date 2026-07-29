using Ardalis.Specification;
using FeedInsight.Domain.JiraSubtasks;

namespace FeedInsight.Application.Features.Jira.Specifications;

public sealed class JiraSubtaskByJiraKeySpec : SingleResultSpecification<JiraSubtask>
{
    public JiraSubtaskByJiraKeySpec(Guid tenantId, string jiraSubtaskKey)
    {
        Query.Where(js => js.TenantId == tenantId && js.JiraSubtaskKey == jiraSubtaskKey);
    }
}