using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Tenants.Queries.GetApiKeys;

public record ApiKeyDto(Guid Id, string Name, string Prefix, DateTime ExpiresAt, bool IsActive, DateTime CreatedAt);

public class GetApiKeysQuery : IRequest<ErrorOr<List<ApiKeyDto>>>
{
}
