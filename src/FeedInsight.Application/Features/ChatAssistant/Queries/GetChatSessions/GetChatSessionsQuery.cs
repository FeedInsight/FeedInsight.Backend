using ErrorOr;

using FeedInsight.Application.Features.ChatAssistant.DTOs;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.ChatAssistant.Queries.GetChatSessions;

public record GetChatSessionsQuery()
    : IRequest<ErrorOr<IReadOnlyList<ChatSessionDto>>>;