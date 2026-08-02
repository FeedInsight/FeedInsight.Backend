using ErrorOr;
using FeedInsight.Application.Features.ChatAssistant.DTOs;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.ChatAssistant.Commands.CreateChatSession;

public record CreateChatSessionCommand(
    string? Title
) : IRequest<ErrorOr<ChatSessionDto>>;