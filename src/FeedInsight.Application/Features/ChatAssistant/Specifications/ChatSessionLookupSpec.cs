using Ardalis.Specification;
using FeedInsight.Domain.Chats;

namespace FeedInsight.Application.Features.ChatAssistant.Specifications;

public class ChatSessionLookupSpec : Specification<ChatSession>
{
    public ChatSessionLookupSpec(
        Guid tenantId,
        Guid userId)
    {
        Query
            .Where(x => x.TenantId == tenantId)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt);
    }
}