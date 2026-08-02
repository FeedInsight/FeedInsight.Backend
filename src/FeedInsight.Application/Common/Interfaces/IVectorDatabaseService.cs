using FeedInsight.Application.Common.Models;

namespace FeedInsight.Application.Common.Interfaces;

public interface IVectorDatabaseService
{
    /// <summary>
    /// Upserts a single vector point with a strongly-typed payload.
    /// </summary>
    Task UpsertPointAsync<TPayload>(
        string collectionName,
        Guid pointId,
        ReadOnlyMemory<float> embedding,
        TPayload payload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts multiple vector points in a single network request.
    /// Highly recommended for performance during ingest.
    /// </summary>
    Task UpsertPointsAsync<TPayload>(
        string collectionName,
        IReadOnlyList<VectorPoint<TPayload>> points,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a specific point by its ID.
    /// </summary>
    Task DeletePointAsync(
        string collectionName,
        Guid pointId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple points by their IDs in a single network request.
    /// </summary>
    Task DeletePointsAsync(
        string collectionName,
        IReadOnlyList<Guid> pointIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a semantic search in the specified collection.
    /// </summary>
    Task<IReadOnlyList<VectorSearchResult<TPayload>>> SearchAsync<TPayload>(
        string collectionName,
        ReadOnlyMemory<float> queryEmbedding,
        int limit = 5,
        MetadataFilter? filter = null,
        CancellationToken cancellationToken = default);
}
