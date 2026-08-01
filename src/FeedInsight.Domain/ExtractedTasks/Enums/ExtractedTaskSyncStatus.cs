namespace FeedInsight.Domain.ExtractedTasks.Enums;

public enum ExtractedTaskSyncStatus
{
    /// <summary>
    /// Task has just been extracted by the Router Agent, but hasn't been matched to a Story yet.
    /// </summary>
    Unassigned = 0,

    /// <summary>
    /// Task has been matched to a User Story. Waiting for the Product Owner to approve it.
    /// </summary>
    PendingPOReview = 1,

    /// <summary>
    /// Product Owner approved it, and it has been pushed to Jira as a sub-task.
    /// </summary>
    SyncedAsSubtask = 2,

    /// <summary>
    /// Product Owner decided this task is a duplicate or not useful.
    /// </summary>
    Ignored = 3
}