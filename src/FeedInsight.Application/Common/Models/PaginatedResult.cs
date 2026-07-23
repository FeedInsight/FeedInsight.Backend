namespace FeedInsight.Application.Common.Models;

/// <summary>
/// A generic wrapper used purely inside the Application layer to pass
/// paginated data and the total count back to the API layer.
/// </summary>
public record PaginatedResult<T>(
    List<T> Items,
    int TotalCount
);
