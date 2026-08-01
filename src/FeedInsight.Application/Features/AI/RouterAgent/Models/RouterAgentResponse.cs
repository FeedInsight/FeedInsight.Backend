using System.Text.Json.Serialization;

namespace FeedInsight.Application.Features.AI.RouterAgent.Models;

public class RouterAgentResponse
{
    [JsonPropertyName("overallSentiment")]
    public string OverallSentiment { get; set; } = "Neutral";

    [JsonPropertyName("tasks")]
    public List<ExtractedTaskResult> Tasks { get; set; } = new();
}