using Ardalis.Specification;
using FeedInsight.Domain.Tenants;

namespace FeedInsight.Application.Features.Tenants.Specifications;

public sealed class TenantLookupCountSpec : Specification<Tenant>
{
    public TenantLookupCountSpec(string? searchTerm)
    {
        Query.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            Query.Where(t => t.CompanyName.Contains(searchTerm));
        }
    }
}
