using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.ChatAssistant.Commands.RenameChatSession;

public record RenameChatSessionCommand(
    Guid SessionId,
    string Title
) : IRequest<ErrorOr<Success>>;