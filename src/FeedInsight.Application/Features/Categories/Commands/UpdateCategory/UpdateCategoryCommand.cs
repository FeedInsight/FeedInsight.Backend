using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description
) : IRequest<ErrorOr<Success>>;