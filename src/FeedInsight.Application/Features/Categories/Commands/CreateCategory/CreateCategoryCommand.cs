using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string? Description
) : IRequest<ErrorOr<Guid>>;
