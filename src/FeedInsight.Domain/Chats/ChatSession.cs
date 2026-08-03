using FeedInsight.Domain.Chats.Enums;
using FeedInsight.Domain.Common.Models;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Users;

namespace FeedInsight.Domain.Chats;

public class ChatSession : Entity
{
    public Guid TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public string Title { get; private set; } = "New Conversation";

    private readonly List<ChatMessage> _messages = new();

    public IReadOnlyCollection<ChatMessage> Messages => _messages.AsReadOnly();

    private ChatSession() { } 

    public ChatSession(
        Guid tenantId,
        Guid userId)
    {
        TenantId = tenantId;
        UserId = userId;
    }

    public void AddMessage(ChatRole role, string content)
    {
        _messages.Add(new ChatMessage(
            TenantId,
            Id,
            role,
            content));
    }

    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Session title cannot be empty.");
        }

        Title = title;
    }

    public void Delete()
    {
        SoftDelete();

        foreach (var message in _messages)
        {
            message.SoftDelete();
        }
    }
}