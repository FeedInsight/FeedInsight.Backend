using FluentValidation;

namespace FeedInsight.Application.Features.Feeds.Commands.CreateFeed;

public class CreateFeedCommandValidator: AbstractValidator<CreateFeedCommand>
{
    public CreateFeedCommandValidator()
    {
        RuleFor(x => x.Title)
        .Cascade(CascadeMode.Continue)
        .NotEmpty()
            .WithMessage("Title cannot be empty.")
            .WithErrorCode("Feed.Validation.MissingTitle") // Optional: Gives the frontend a programmatic code
        .MaximumLength(100)
            .WithMessage("Title cannot exceed 100 characters.")
            .WithErrorCode("Feed.Validation.TitleTooLong");

        RuleFor(x => x.Description) // Changed from Content to Description to match the Command
            .Cascade(CascadeMode.Continue)
            .NotEmpty()
                .WithMessage("Description cannot be empty.")
                .WithErrorCode("Feed.Validation.MissingDescription");

        RuleFor(x => x.Url)
            .Cascade(CascadeMode.Continue)
            .NotEmpty()
                .WithMessage("URL cannot be empty.")
                .WithErrorCode("Feed.Validation.MissingUrl");

        // Note: We don't need complex Regex URL validation here! 
        // Our `FeedUrl` Value Object inside the Domain layer handles the strict formatting rules.
        // This validator just ensures the user didn't send a completely blank request.
    }
}