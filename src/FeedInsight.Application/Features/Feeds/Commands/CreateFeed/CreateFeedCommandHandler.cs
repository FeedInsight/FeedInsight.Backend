using ErrorOr;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Feeds;
using FeedInsight.Domain.Feeds.Enums;
using FeedInsight.Domain.Feeds.ValueObjects;
using static FeedInsight.Domain.Common.Errors.Errors;

namespace FeedInsight.Application.Features.Feeds.Commands.CreateFeed;

public class CreateFeedCommandHandler : IRequestHandler<CreateFeedCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> HandleAsync(CreateFeedCommand request, CancellationToken cancellationToken = default)
    {
        /*
         * 1. SYNCHRONOUS BUSINESS RULES
         */

        // Check forbidden words using centralized error
        if (request.Title.Equals("Spam", StringComparison.OrdinalIgnoreCase))
        {
            return Errors.Feeds.InvalidTitle;
        }

        // Validate and Create the Value Object
        var urlResult = FeedUrl.Create(request.Url);
        if (urlResult.IsError)
        {
            // This returns the Errors.Feeds.InvalidUrl error generated inside the record!
            return urlResult.Errors;
        }

        /*
         * 2. ASYNCHRONOUS DATA CHECKS (Simulated)
         */

        // Simulate checking the database for uniqueness
        if (request.Title == "Existing Title")
        {
            return Errors.Feeds.TitleTaken;
        }

        // Simulate external service failure
        bool isDatabaseDown = false;
        if (isDatabaseDown)
        {
            return Error.Unexpected(
                code: "Database.Failure",
                description: "Could not connect to the database.");
        }

        /*
         * 3. ENTITY CREATION AND PERSISTENCE
         */

        // Create the Entity. 
        // Notice how we use urlResult.Value. It is GUARANTEED to be a valid URL here.
        var feed = new Feed
        {
            Title = request.Title,
            Description = request.Description,
            Url = urlResult.Value,
            Status = FeedStatus.Active
        };

        // TODO: In the future, this is where you will add your repository logic:
        // await _feedRepository.AddAsync(feed, cancellationToken);
        // await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. RETURN SUCCESS
        return feed.Id;
    }
}