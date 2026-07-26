using Ardalis.Specification;
using FeedInsight.Domain.Tenants;

namespace FeedInsight.Application.Features.Tenants.Specifications;

public sealed class TenantLookupSpec : Specification<Tenant>
{
    public TenantLookupSpec(
        string? searchTerm,
        int? page,
        int? pageSize)
    {
        Query
            .AsNoTracking()
            .OrderBy(t => t.CompanyName);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            Query.Where(t => t.CompanyName.Contains(searchTerm));
        }

        if (page.HasValue && pageSize.HasValue)
        {
            Query.Skip((page.Value - 1) * pageSize.Value)
                 .Take(pageSize.Value);
        }
    }
}