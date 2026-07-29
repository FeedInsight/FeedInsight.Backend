namespace FeedInsight.Domain.UserStories.Enums;

public enum UserStorySource
{
    /// <summary>
    /// Drafted internally by the FeedInsight Daily Triage AI.
    /// </summary>
    FeedInsight = 0,

    /// <summary>
    /// Imported directly from the customer's Jira backlog.
    /// </summary>
    Jira = 1
}