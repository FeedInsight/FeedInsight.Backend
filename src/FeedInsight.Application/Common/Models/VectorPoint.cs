using System;

namespace FeedInsight.Application.Common.Models;

/// <summary>
/// Represents a single point for bulk insertions.
/// </summary>
public record VectorPoint<TPayload>(
    Guid Id, 
    ReadOnlyMemory<float> Embedding, 
    TPayload Payload);
