using ErrorOr;
using FeedInsight.Domain.Common.Errors;

namespace FeedInsight.Domain.Feeds.ValueObjects;

public record FeedUrl
{
    // Using 'init' ensures immutability (it cannot be changed after creation)
    public string Value { get; init; }

    // Private constructor so it can ONLY be created via the Create method (or EF Core)
    private FeedUrl(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory method that guarantees the URL is valid before creating the object.
    /// </summary>
    public static ErrorOr<FeedUrl> Create(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return Errors.Feeds.InvalidUrl;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return Errors.Feeds.InvalidUrl;
        }

        return new FeedUrl(url);
    }

    // Allows implicit conversion to string if needed (e.g., feed.Url.Value becomes just feed.Url)
    public override string ToString() => Value;
}