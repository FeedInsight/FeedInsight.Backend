namespace FeedInsight.API.Contracts.Responses;

/// <summary>
/// A generic envelope for single-resource API responses.
/// </summary>
public record ApiResponse<T>(
    T Data,
    object? Meta = null
);
