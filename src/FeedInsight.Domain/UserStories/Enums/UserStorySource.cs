using System.Text.Json.Serialization;

namespace FeedInsight.Domain.UserStories.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
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