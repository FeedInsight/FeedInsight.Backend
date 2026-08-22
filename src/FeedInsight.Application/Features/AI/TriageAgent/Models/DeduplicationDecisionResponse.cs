using System;

namespace FeedInsight.Application.Features.AI.TriageAgent.Models;

public class DeduplicationDecisionResponse
{
    public bool IsDuplicate { get; set; }
    public Guid? DuplicateOfStoryId { get; set; }
    public string Reasoning { get; set; } = string.Empty;
}
