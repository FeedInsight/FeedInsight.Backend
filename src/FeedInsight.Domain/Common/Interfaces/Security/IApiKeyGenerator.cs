namespace FeedInsight.Domain.Common.Interfaces.Security;

public interface IApiKeyGenerator
{
    /// <summary>
    /// Generates a secure, random string to be used as a plain-text API key.
    /// Example output: "fi_live_8f73b2d19c0a4e5f..."
    /// </summary>
    string Generate();
}
