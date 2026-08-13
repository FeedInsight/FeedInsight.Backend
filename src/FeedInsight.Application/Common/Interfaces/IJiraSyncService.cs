using FeedInsight.Application.Features.Jira.Models;
using FeedInsight.Domain.UserStories;

namespace FeedInsight.Application.Common.Interfaces;

public interface IJiraSyncService
{
    /// <summary>
    /// Fetches all active Epics, Stories, and Subtasks from Jira and seeds the FeedInsight database.
    /// Expected to be a long-running background task.
    /// </summary>
    Task TriggerInitialBulkSyncAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<JiraIssueDto?> GetIssueByKeyAsync(Guid tenantId, string issueKey, CancellationToken cancellationToken = default);

    Task<string?> PushStoryToJiraAsync(Guid tenantId, UserStory story, CancellationToken cancellationToken = default);
}