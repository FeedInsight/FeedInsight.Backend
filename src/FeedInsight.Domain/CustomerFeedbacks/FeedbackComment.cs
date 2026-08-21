using FeedInsight.Domain.Common.Models;
using FeedInsight.Domain.Users;

namespace FeedInsight.Domain.CustomerFeedbacks;

public class FeedbackComment : Entity
{
    public Guid CustomerFeedbackId { get; private set; }

    public CustomerFeedback CustomerFeedback { get; private set; } = null!;

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public string Content { get; private set; }

    private FeedbackComment() { }

    public FeedbackComment(
        Guid customerFeedbackId,
        Guid userId,
        string content)
    {
        if (customerFeedbackId == Guid.Empty)
            throw new ArgumentException("Customer feedback ID cannot be empty.");

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Comment content cannot be empty.");

        CustomerFeedbackId = customerFeedbackId;
        UserId = userId;
        Content = content;
    }
}