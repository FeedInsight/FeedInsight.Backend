using ErrorOr;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;

namespace FeedInsight.Application.Features.Feeds.Queries.GetFeeds;

public class GetFeedsQueryHandler : IRequestHandler<GetFeedsQuery, ErrorOr<PaginatedResult<FeedDto>>>
{
    public async Task<ErrorOr<PaginatedResult<FeedDto>>> HandleAsync(GetFeedsQuery request, CancellationToken cancellationToken = default)
    {
        /* 
         * 1. BUSINESS RULE ORCHESTRATION 
         * Using ErrorOr to handle various business cases before touching the database.
         */

        // Case A: Premium feature restriction (Simulating a Forbidden error)
        if (request.PageSize > 50)
        {
            return Errors.Feeds.PageSizeExceeded;
        }

        // Case B: Content moderation (Simulating a Failure error)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm) && request.SearchTerm.Contains("hack", StringComparison.OrdinalIgnoreCase))
        {
            return Errors.Feeds.InvalidSearch;
        }


        /* 
         * 2. DATA FETCHING (Simulated Database)
         */

        // Imagine this is _dbContext.Feeds.AsQueryable()
        var dummyDatabase = GenerateDummyFeeds();

        // Apply Search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            dummyDatabase = dummyDatabase
                .Where(f => f.Title.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Calculate Total Count BEFORE applying Skip/Take
        var totalCount = dummyDatabase.Count;

        // Apply Pagination
        var paginatedItems = dummyDatabase
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        /* 
         * 3. RETURN RESULT
         */
        return new PaginatedResult<FeedDto>(paginatedItems, totalCount);
    }

    // Helper method just to generate fake data for this example
    private static List<FeedDto> GenerateDummyFeeds()
    {
        var list = new List<FeedDto>();
        for (int i = 1; i <= 45; i++)
        {
            list.Add(new FeedDto(Guid.NewGuid(), $"Feed Title {i}", $"Content for feed {i}", DateTime.UtcNow.AddDays(-i)));
        }
        return list;
    }
}
