using Ardalis.Specification;
using FeedInsight.Domain.Tenants;

namespace FeedInsight.Application.Features.Tenants.Specifications;

public class TenantWithApiKeysSpec : Specification<Tenant>, ISingleResultSpecification
{
    public TenantWithApiKeysSpec(Guid tenantId)
    {
        Query.Where(t => t.Id == tenantId)
             .Include(t => t.ApiKeys);
    }
}
