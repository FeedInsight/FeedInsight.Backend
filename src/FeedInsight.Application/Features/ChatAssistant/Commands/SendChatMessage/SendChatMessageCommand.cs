using ErrorOr;
using FeedInsight.Application.Features.ChatAssistant.DTOs;
using FeedInsight.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.ChatAssistant.Commands.SendChatMessage
{
    public record SendChatMessageCommand(
    Guid SessionId,
    string Content
) : IRequest<ErrorOr<SendMessageResponseDto>>;
}
