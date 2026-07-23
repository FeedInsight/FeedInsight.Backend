using FeedInsight.Application.Messaging;
using ErrorOr;

namespace FeedInsight.Application.Features.Feeds.Commands.CreateFeed;

public record CreateFeedCommand(
    string Title,
    string Description,
    string Url
) : IRequest<ErrorOr<Guid>>;