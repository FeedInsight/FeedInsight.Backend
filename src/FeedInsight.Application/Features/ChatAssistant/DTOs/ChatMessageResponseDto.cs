using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.ChatAssistant.DTOs
{
    public record SendMessageResponseDto(
    ChatMessageDto UserMessage,
    ChatMessageDto AssistantMessage
);
}
