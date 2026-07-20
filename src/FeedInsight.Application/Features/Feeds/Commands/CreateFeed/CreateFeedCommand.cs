using FeedInsight.Application.Messaging;
using ErrorOr;

namespace FeedInsight.Application.Features.Feeds.Commands.CreateFeed;

public class CreateFeedCommand : IRequest<ErrorOr<Guid>>
{
    public string? Title { get; set; }
    public string? Content { get; set; }
}
