namespace FeedInsight.Application.Features.ChatAssistant.DTOs;

public record ChatMessageDto(
    Guid Id,
    string SenderRole,
    string Content,
    DateTime CreatedAt
);
