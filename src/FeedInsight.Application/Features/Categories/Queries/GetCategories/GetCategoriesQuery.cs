using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<ErrorOr<List<CategoryDto>>>;