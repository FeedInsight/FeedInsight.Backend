using Ardalis.Specification;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Specifications;

public sealed class UsersByTenantIdSpec : Specification<User>
{
    public UsersByTenantIdSpec(Guid tenantId)
    {
        Query
            .Where(u => u.TenantId == tenantId)
            .Include(u => u.RefreshTokens);
    }
}