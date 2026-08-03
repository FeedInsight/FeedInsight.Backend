using ErrorOr;

using FeedInsight.Application.Features.ChatAssistant.DTOs;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.ChatAssistant.Queries.GetChatSessionById;

public record GetChatSessionByIdQuery(Guid SessionId)
    : IRequest<ErrorOr<ChatSessionDetailsDto>>;