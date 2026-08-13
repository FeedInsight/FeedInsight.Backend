using FeedInsight.Domain.Common.Models;
using FeedInsight.Domain.CustomerFeedbacks.Events;
using FeedInsight.Domain.Users;

namespace FeedInsight.Domain.CustomerFeedbacks;

public class CustomerFeedback : Entity
{
    public Guid TenantId { get; private set; }

    public string RawContent { get; private set; }

    public Guid? SubmitterUserId { get; private set; }
    public User? SubmitterUser { get; private set; }

    public string? SubmitterEmail { get; private set; }

    public string? MetadataJson { get; private set; }

    public string? OverallSentiment { get; private set; }

    public bool IsProcessedByRouter { get; private set; }

    private CustomerFeedback() { }

    public CustomerFeedback(
        Guid tenantId,
        string rawContent,
        string? submitterEmail = null,
        string? metadataJson = null,
        Guid? submitterUserId = null)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
            throw new ArgumentException("Feedback content cannot be empty.");

        TenantId = tenantId;
        RawContent = rawContent;
        SubmitterEmail = submitterEmail;
        MetadataJson = metadataJson;
        SubmitterUserId = submitterUserId;

        IsProcessedByRouter = false;

        AddDomainEvent(new CustomerFeedbackCreatedEvent(TenantId, Id));
    }

    /// <summary>
    /// Called by the Semantic Kernel background worker once the AI has successfully 
    /// split this raw text into actionable insights and pushed them to Qdrant.
    /// </summary>
    public void MarkAsProcessed(string overallSentiment)
    {
        IsProcessedByRouter = true;
        OverallSentiment = overallSentiment;
    }
}
