using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Models;
using FeedInsight.Domain.UserStories.Enums;
using FeedInsight.Domain.UserStories.Events;

namespace FeedInsight.Domain.UserStories;

public class UserStory : Entity
{
    public Guid TenantId { get; private set; }

    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public UserStorySource Source { get; private set; }

    public string? JiraTicketKey { get; private set; }

    public string Title { get; private set; }

    public string? AcceptanceCriteria { get; private set; }

    public int UrgencyScore { get; private set; }

    public UserStoryStatus Status { get; private set; }

    private UserStory() { } // EF Core

    public UserStory(
        Guid tenantId,
        Guid categoryId,
        UserStorySource source,
        string title,
        string? acceptanceCriteria = null,
        string? jiraTicketKey = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.");

        TenantId = tenantId;
        CategoryId = categoryId;
        Source = source;
        Title = title;
        AcceptanceCriteria = acceptanceCriteria;
        JiraTicketKey = jiraTicketKey;

        UrgencyScore = 0;
        Status = source == UserStorySource.Jira ? UserStoryStatus.Synced : UserStoryStatus.Draft;

        AddDomainEvent(new UserStoryCreatedEvent(TenantId, Id));
    }

    public void UpdateFromJiraWebhook(string title, string? acceptanceCriteria, UserStoryStatus status)
    {
        Title = title;
        AcceptanceCriteria = acceptanceCriteria;
        Status = status;

        AddDomainEvent(new UserStoryUpdatedEvent(TenantId, Id));
    }

    public void UpdateCategory(Guid categoryId)
    {
        CategoryId = categoryId;
        // No domain event is added here to avoid recursive event handling. 
        // This is primarily updated by AI handlers.
    }

    public void MarkAsSyncedToJira(string jiraTicketKey)
    {
        JiraTicketKey = jiraTicketKey;
        Status = UserStoryStatus.Synced;
        AddDomainEvent(new UserStoryUpdatedEvent(TenantId, Id));
    }

    public void UpdateDetails(string title, string? acceptanceCriteria)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.");

        Title = title;
        AcceptanceCriteria = acceptanceCriteria;
        AddDomainEvent(new UserStoryUpdatedEvent(TenantId, Id));
    }

    public void IncreaseUrgency(int amount = 1)
    {
        UrgencyScore += amount;
        AddDomainEvent(new UserStoryUrgencyIncreasedEvent(TenantId, Id));
    }
}