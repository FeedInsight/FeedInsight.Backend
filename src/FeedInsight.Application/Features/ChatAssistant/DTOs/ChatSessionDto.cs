namespace FeedInsight.Application.Features.ChatAssistant.DTOs;

public record ChatSessionDto(
    Guid Id,
    string Title,
    DateTime CreatedAt
);