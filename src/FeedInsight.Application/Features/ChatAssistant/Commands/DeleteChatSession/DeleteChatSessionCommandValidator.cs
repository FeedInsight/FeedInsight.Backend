using FluentValidation;

namespace FeedInsight.Application.Features.ChatAssistant.Commands.DeleteChatSession;

public class DeleteChatSessionCommandValidator
    : AbstractValidator<DeleteChatSessionCommand>
{
    public DeleteChatSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty();
    }
}