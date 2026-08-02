using Ardalis.Specification;
using FeedInsight.Domain.Chats;

using System.Collections;

namespace FeedInsight.Application.Features.ChatAssistant.Specifications;

public class ChatMessagesCountSpec
    : Specification<ChatMessage>
{
    public ChatMessagesCountSpec(Guid sessionId)
    {
        Query.Where(x => x.ChatSessionId == sessionId);
    }
}