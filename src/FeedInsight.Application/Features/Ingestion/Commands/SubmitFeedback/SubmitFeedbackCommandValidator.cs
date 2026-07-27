using FluentValidation;

namespace FeedInsight.Application.Features.Ingestion.Commands.SubmitFeedback;

public class SubmitFeedbackCommandValidator : AbstractValidator<SubmitFeedbackCommand>
{
    public SubmitFeedbackCommandValidator()
    {
        RuleFor(x => x.RawContent)
            .NotEmpty().WithMessage("Feedback content cannot be empty.");

        RuleFor(x => x.SubmitterEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.SubmitterEmail))
            .WithMessage("Submitter email must be a valid email address.");
    }
}
