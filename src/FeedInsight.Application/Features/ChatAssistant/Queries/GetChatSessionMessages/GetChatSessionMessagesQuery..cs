using ErrorOr;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.ChatAssistant.DTOs;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.ChatAssistant.Queries.GetChatSessionMessages;

public record GetChatSessionMessagesQuery(
    Guid SessionId,
    int? Page = null,
    int? PageSize = null
)
: IRequest<ErrorOr<PaginatedResult<ChatMessageDto>>>;