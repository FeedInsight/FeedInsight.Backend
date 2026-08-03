namespace FeedInsight.Application.Features.Categories.Queries.GetCategories;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemDefault,
    DateTime CreatedAt
);