using Ardalis.Specification;
using FeedInsight.Domain.Categories;

namespace FeedInsight.Application.Features.Categories.Specifications;

public sealed class CategoryByIdAndTenantSpec : SingleResultSpecification<Category>
{
    public CategoryByIdAndTenantSpec(Guid id, Guid tenantId)
    {
        Query.Where(c => c.Id == id && c.TenantId == tenantId);
    }
}