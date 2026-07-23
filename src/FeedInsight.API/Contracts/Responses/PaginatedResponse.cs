namespace FeedInsight.API.Contracts.Responses;

/// <summary>
/// A generic envelope for list-based API responses, including pagination details.
/// </summary>
public record PaginatedResponse<T>(
    IEnumerable<T> Data,
    PaginationMetadata Pagination,
    object? Meta = null
);
