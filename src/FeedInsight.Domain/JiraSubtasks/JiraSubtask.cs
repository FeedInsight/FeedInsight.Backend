using FeedInsight.Domain.Common.Models;

namespace FeedInsight.Domain.JiraSubtasks;

public class JiraSubtask : Entity
{
    public Guid TenantId { get; private set; }

    public Guid UserStoryId { get; private set; }

    public string JiraSubtaskKey { get; private set; }

    public string Title { get; private set; }

    public string Status { get; private set; }

    private JiraSubtask() { }

    public JiraSubtask(Guid tenantId, Guid userStoryId, string jiraSubtaskKey, string title, string status)
    {
        TenantId = tenantId;
        UserStoryId = userStoryId;
        JiraSubtaskKey = jiraSubtaskKey;
        Title = title;
        Status = status;
    }

    public void UpdateFromWebhook(string title, string status)
    {
        Title = title;
        Status = status;
    }
}