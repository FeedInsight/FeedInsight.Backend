using System;

namespace FeedInsight.Application.Common.Models;

/// <summary>
/// A generic search result that automatically deserializes the payload into your domain model.
/// </summary>
public record VectorSearchResult<TPayload>(
    Guid PointId,
    float Score,
    TPayload Payload);
