using Ardalis.Specification;
using FeedInsight.Domain.Chats;

using System.Collections;

namespace FeedInsight.Application.Features.ChatAssistant.Specifications;

public class ChatMessagesSpec
    : Specification<ChatMessage>
{
    public ChatMessagesSpec(
        Guid sessionId,
        int? page,
        int? pageSize)
    {
        Query
            .Where(x => x.ChatSessionId == sessionId)
            .OrderBy(x => x.CreatedAt);


        if (page.HasValue && pageSize.HasValue)
        {
            Query
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }
    }
}