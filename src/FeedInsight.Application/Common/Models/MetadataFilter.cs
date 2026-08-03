using System.Collections.Generic;

namespace FeedInsight.Application.Common.Models;

/// <summary>
/// An abstraction for filtering. You can expand this based on the specific Vector DB you are using.
/// </summary>
public record MetadataFilter
{
    // Examples of how you might structure this depending on your needs:
    public IReadOnlyDictionary<string, object>? MustMatch { get; init; }
    public IReadOnlyDictionary<string, object>? MustNotMatch { get; init; }
}
