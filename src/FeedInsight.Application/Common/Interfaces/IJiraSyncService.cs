namespace FeedInsight.Application.Common.Interfaces;

public interface IJiraSyncService
{
    /// <summary>
    /// Fetches all active Epics, Stories, and Subtasks from Jira and seeds the FeedInsight database.
    /// Expected to be a long-running background task.
    /// </summary>
    Task TriggerInitialBulkSyncAsync(Guid tenantId, CancellationToken cancellationToken = default);
}