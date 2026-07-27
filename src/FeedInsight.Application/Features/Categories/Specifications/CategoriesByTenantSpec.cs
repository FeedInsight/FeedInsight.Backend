using Ardalis.Specification;
using FeedInsight.Domain.Categories;

namespace FeedInsight.Application.Features.Categories.Specifications;

public sealed class CategoriesByTenantSpec : Specification<Category>
{
    public CategoriesByTenantSpec(Guid tenantId)
    {
        Query.AsNoTracking()
             .Where(c => c.TenantId == tenantId);
    }
}