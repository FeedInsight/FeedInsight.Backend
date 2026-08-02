using FluentValidation;

namespace FeedInsight.Application.Features.ChatAssistant.Commands.RenameChatSession;

public class RenameChatSessionCommandValidator
    : AbstractValidator<RenameChatSessionCommand>
{
    public RenameChatSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(150);
    }
}