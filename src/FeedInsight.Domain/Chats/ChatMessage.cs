using FeedInsight.Domain.Chats.Enums;
using FeedInsight.Domain.Common.Models;
using FeedInsight.Domain.Tenants;

namespace FeedInsight.Domain.Chats;

public class ChatMessage : Entity
{
    public Guid TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }

    public Guid ChatSessionId { get; private set; }
    public ChatSession? ChatSession { get; private set; }

    public ChatRole SenderRole { get; private set; }

    public string Content { get; private set; }

    private ChatMessage() { } // constructor for ef-core

    public ChatMessage(
        Guid tenantId,
        Guid chatSessionId,
        ChatRole senderRole,
        string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be empty.");
        }

        TenantId = tenantId;
        ChatSessionId = chatSessionId;
        SenderRole = senderRole;
        Content = content;
    }
}