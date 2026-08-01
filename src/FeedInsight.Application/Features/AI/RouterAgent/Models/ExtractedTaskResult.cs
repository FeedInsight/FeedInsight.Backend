using System.Text.Json.Serialization;

namespace FeedInsight.Application.Features.AI.RouterAgent.Models;

/// <summary>
/// Represents the exact JSON structure we are forcing the LLM to return.
/// </summary>
public class ExtractedTaskResult
{
    [JsonPropertyName("extractedIntent")]
    public string ExtractedIntent { get; set; } = string.Empty;

    [JsonPropertyName("categoryId")]
    public Guid CategoryId { get; set; }

    [JsonPropertyName("technicalKeywords")]
    public string TechnicalKeywords { get; set; } = string.Empty;
}