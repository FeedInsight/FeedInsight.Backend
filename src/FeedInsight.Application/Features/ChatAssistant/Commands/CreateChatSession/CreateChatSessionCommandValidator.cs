using FluentValidation;

namespace FeedInsight.Application.Features.ChatAssistant.Commands.CreateChatSession;

public class CreateChatSessionCommandValidator
    : AbstractValidator<CreateChatSessionCommand>
{
    public CreateChatSessionCommandValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(150);

        RuleFor(x => x.Title)
            .NotEmpty()
            .When(x => x.Title is not null);
    }
}