using System.Text.Json.Serialization;

namespace FeedInsight.Domain.UserStories.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserStoryStatus
{
    Draft = 0,
    PendingReview = 1,
    Synced = 2,
    Closed = 3
}