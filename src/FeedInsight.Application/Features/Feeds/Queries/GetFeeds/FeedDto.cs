namespace FeedInsight.Application.Features.Feeds.Queries.GetFeeds;

/// <summary>
/// The Data Transfer Object (DTO) that represents the data we want to return to the user.
/// </summary>
public record FeedDto(
    Guid Id,
    string Title,
    string Content,
    DateTime CreatedAt
);