using FeedInsight.Domain.Common.Models;
using FeedInsight.Domain.ExtractedTasks.Enums;
using FeedInsight.Domain.ExtractedTasks.Events;

namespace FeedInsight.Domain.ExtractedTasks;

/// <summary>
/// Represents a single, actionable technical intent extracted from a raw CustomerFeedback.
/// The Id of this entity serves as the exact Point ID in the Qdrant tasks_collection.
/// </summary>
public class ExtractedTask : Entity
{
    public Guid TenantId { get; private set; }

    public Guid CustomerFeedbackId { get; private set; }

    public Guid CategoryId { get; private set; }

    public Guid? UserStoryId { get; private set; }

    public string ExtractedIntent { get; private set; }

    public string? TechnicalKeywords { get; private set; }

    public ExtractedTaskSyncStatus SyncStatus { get; private set; }

    public string? JiraSubtaskKey { get; private set; }

    private ExtractedTask() { } // constructor for ef-core

    public ExtractedTask(
        Guid tenantId,
        Guid customerFeedbackId,
        Guid categoryId,
        string extractedIntent,
        string? technicalKeywords)
    {
        if (string.IsNullOrWhiteSpace(extractedIntent))
            throw new ArgumentException("Extracted intent cannot be empty.");

        TenantId = tenantId;
        CustomerFeedbackId = customerFeedbackId;
        CategoryId = categoryId;
        ExtractedIntent = extractedIntent;
        TechnicalKeywords = technicalKeywords;

        // Always starts as unassigned until semantic matching or triage occurs
        SyncStatus = ExtractedTaskSyncStatus.Unassigned;
        
        AddDomainEvent(new ExtractedTaskCreatedEvent(TenantId, Id));
    }


    /// <summary>
    /// Links this task to an existing or newly drafted User Story.
    /// </summary>
    public void AssignToStory(Guid userStoryId)
    {
        UserStoryId = userStoryId;
        SyncStatus = ExtractedTaskSyncStatus.PendingPOReview;
    }

    /// <summary>
    /// Marks the task as pushed to Jira.
    /// </summary>
    public void MarkAsSynced(string jiraSubtaskKey)
    {
        if (string.IsNullOrWhiteSpace(jiraSubtaskKey))
            throw new ArgumentException("Jira key must be provided when syncing.");

        JiraSubtaskKey = jiraSubtaskKey;
        SyncStatus = ExtractedTaskSyncStatus.SyncedAsSubtask;
    }

    /// <summary>
    /// Allows a Product Owner to dismiss an irrelevant or duplicate extracted task.
    /// </summary>
    public void Ignore()
    {
        SyncStatus = ExtractedTaskSyncStatus.Ignored;
    }
}