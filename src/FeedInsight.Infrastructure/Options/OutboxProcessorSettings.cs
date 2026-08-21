namespace FeedInsight.Infrastructure.Options;

public class OutboxProcessorSettings
{
    public const string SectionName = "OutboxProcessor";

    public int MaxRetries { get; set; }
    public int BaseDelayMilliseconds { get; set; }
}

