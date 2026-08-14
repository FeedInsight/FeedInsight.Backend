using Ardalis.Specification;
using FeedInsight.Domain.Categories;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Specifications;

public sealed class CategoriesByIdsSpec : Specification<Category>
{
    public CategoriesByIdsSpec(List<Guid> categoryIds)
    {
        Query.AsNoTracking().Where(x => categoryIds.Contains(x.Id));
    }
}