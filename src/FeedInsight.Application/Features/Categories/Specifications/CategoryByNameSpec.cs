using Ardalis.Specification;
using FeedInsight.Domain.Categories;

namespace FeedInsight.Application.Features.Categories.Specifications;

public sealed class CategoryByNameSpec : SingleResultSpecification<Category>
{
    public CategoryByNameSpec(Guid tenantId, string name)
    {
        Query.AsNoTracking()
             .Where(c => c.TenantId == tenantId && c.Name.ToLower() == name.ToLower());
    }
}