using Ardalis.Specification;
using FeedInsight.Domain.Chats;

namespace FeedInsight.Application.Features.ChatAssistant.Specifications;

public class ChatSessionByIdSpec : Specification<ChatSession>
{
    public ChatSessionByIdSpec(
        Guid tenantId,
        Guid userId,
        Guid sessionId)
    {
        Query
            .Where(x =>
                x.Id == sessionId &&
                x.TenantId == tenantId &&
                x.UserId == userId)
            .Include(x => x.Messages);
    }
}