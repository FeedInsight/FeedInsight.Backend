using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(
    Guid Id
) : IRequest<ErrorOr<Success>>;