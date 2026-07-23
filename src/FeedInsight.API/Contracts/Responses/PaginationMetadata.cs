namespace FeedInsight.API.Contracts.Responses;

/// <summary>
/// Contains navigation and counting data for paginated lists.
/// </summary>
public record PaginationMetadata(
    int CurrentPage,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);