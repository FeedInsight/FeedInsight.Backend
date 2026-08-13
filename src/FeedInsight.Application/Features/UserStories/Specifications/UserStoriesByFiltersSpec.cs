using Ardalis.Specification;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Enums;
using System;

namespace FeedInsight.Application.Features.UserStories.Specifications;

public class UserStoriesByFiltersSpec : Specification<UserStory>
{
    public UserStoriesByFiltersSpec(
        Guid tenantId,
        UserStorySource? source,
        bool? isSynced,
        string? searchTerm,
        Guid? categoryId,
        int? pageNumber = null,
        int? pageSize = null)
    {
        Query.Where(x => x.TenantId == tenantId);

        if (source.HasValue)
        {
            Query.Where(x => x.Source == source.Value);
        }

        if (isSynced.HasValue)
        {
            if (isSynced.Value)
            {
                Query.Where(x => x.Status == UserStoryStatus.Synced);
            }
            else
            {
                Query.Where(x => x.Status != UserStoryStatus.Synced);
            }
        }

        if (categoryId.HasValue)
        {
            Query.Where(x => x.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerTerm = searchTerm.ToLower();
            Query.Where(x => x.Title.ToLower().Contains(lowerTerm) || 
                            (x.AcceptanceCriteria != null && x.AcceptanceCriteria.ToLower().Contains(lowerTerm)));
        }
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            var skip = (pageNumber.Value - 1) * pageSize.Value;
            Query.Skip(skip).Take(pageSize.Value);
        }
    }
}
