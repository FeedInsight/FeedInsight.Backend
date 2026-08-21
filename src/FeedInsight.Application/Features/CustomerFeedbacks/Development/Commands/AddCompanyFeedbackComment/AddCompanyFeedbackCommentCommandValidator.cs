using FluentValidation;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Commands.Development.AddCompanyFeedbackComment;

public class AddCompanyFeedbackCommentCommandValidator
    : AbstractValidator<AddCompanyFeedbackCommentCommand>
{
    public AddCompanyFeedbackCommentCommandValidator()
    {
        RuleFor(x => x.FeedbackId)
            .NotEmpty()
            .WithMessage("Feedback ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment cannot be empty.")
            .MaximumLength(2000)
            .WithMessage("Comment cannot exceed 2000 characters.");
    }
}