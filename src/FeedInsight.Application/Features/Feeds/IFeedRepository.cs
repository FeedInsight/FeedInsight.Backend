using FeedInsight.Domain.Feeds;

namespace FeedInsight.Application.Features.Feeds;

public interface IFeedRepository
{
    Task AddAsync(Feed feed, CancellationToken cancellationToken = default);
}
