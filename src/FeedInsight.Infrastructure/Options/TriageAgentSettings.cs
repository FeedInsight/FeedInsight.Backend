namespace FeedInsight.Infrastructure.Options;

public class TriageAgentSettings
{
    public const string SectionName = "TriageAgent";

    public float DeduplicationSimilarityThreshold { get; set; }
}
