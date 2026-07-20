using FluentValidation;

namespace FeedInsight.Application.Features.Feeds.Commands.CreateFeed
{
    public class CreateFeedCommandValidator: AbstractValidator<CreateFeedCommand>
    {
        public CreateFeedCommandValidator()
        {
            RuleFor(x => x.Title)
            .Cascade(CascadeMode.Continue)
            .NotEmpty().WithMessage("Title cannot be empty.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Content)
                .Cascade(CascadeMode.Continue)
                .NotEmpty().WithMessage("Content cannot be empty.");
        }
    }
}
