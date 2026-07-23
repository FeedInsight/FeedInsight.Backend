using FeedInsight.Application.Features.Feeds;
using FeedInsight.Domain.Feeds;
using FeedInsight.Infrastructure.Persistence.Context;

namespace FeedInsight.Infrastructure.Persistence.Repositories;

public sealed class FeedRepository : IFeedRepository
{
    private readonly FeedInsightDbContext _dbContext;

    public FeedRepository(FeedInsightDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Feed feed, CancellationToken cancellationToken = default)
    {
        await _dbContext.Feeds.AddAsync(feed, cancellationToken);
    }
}
