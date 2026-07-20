using FeedInsight.Application.Messaging;
using ErrorOr;

namespace FeedInsight.Application.Features.Feeds.Commands.CreateFeed;

internal class CreateFeedCommandHandler : IRequestHandler<CreateFeedCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> HandleAsync(CreateFeedCommand request, CancellationToken cancellationToken = default)
    {
        // 1. Business Logic Error: Forbidden word
        if (request.Title.Equals("Spam", StringComparison.OrdinalIgnoreCase))
        {
            return Error.Failure(
                code: "Feed.InvalidTitle",
                description: "The title cannot be 'Spam'.");
        }

        // 2. Simulated Database Check: Conflict
        // Imagine checking a database here...
        if (request.Title == "Existing Title")
        {
            return Error.Conflict(
                code: "Feed.TitleTaken",
                description: "A feed with this title already exists.");
        }

        // 3. Simulated external service failure: Unexpected
        bool isDatabaseDown = false; // logic to check system health
        if (isDatabaseDown)
        {
            return Error.Unexpected(
                code: "Database.Failure",
                description: "Could not connect to the database.");
        }

        // Success logic
        return Guid.NewGuid();
    }
}
