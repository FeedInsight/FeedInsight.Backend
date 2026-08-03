using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.ChatAssistant.Commands.DeleteChatSession;

public record DeleteChatSessionCommand(Guid SessionId)
    : IRequest<ErrorOr<Success>>;