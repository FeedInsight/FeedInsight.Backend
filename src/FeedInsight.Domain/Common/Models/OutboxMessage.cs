namespace FeedInsight.Domain.Common.Models;

public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public string Type { get; init; } = string.Empty;
    
    public string Content { get; init; } = string.Empty;
    
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    
    public DateTime? ProcessedOnUtc { get; set; }
    
    public string? Error { get; set; }

    public int RetryCount { get; set; } = 0;

    public DateTime? NextRetryUtc { get; set; }
}
