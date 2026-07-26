using Ardalis.Specification;
using FeedInsight.Domain.Tenants;

namespace FeedInsight.Application.Features.Ingestion.Specifications;

public class TenantByApiKeyHashSpec : Specification<Tenant>, ISingleResultSpecification
{
    public TenantByApiKeyHashSpec(string keyHash)
    {
        Query.Where(t => t.ApiKeys.Any(k => k.KeyHash == keyHash && !k.IsDeleted && k.ExpiresAt > DateTime.UtcNow))
             .Include(t => t.ApiKeys);
    }
}
