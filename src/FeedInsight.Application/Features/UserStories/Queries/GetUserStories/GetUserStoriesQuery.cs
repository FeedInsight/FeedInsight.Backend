using ErrorOr;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.UserStories.Enums;

namespace FeedInsight.Application.Features.UserStories.Queries.GetUserStories;

public record GetUserStoriesQuery(
    UserStorySource? Source = null,
    bool? IsSynced = null,
    string? SearchTerm = null,
    Guid? CategoryId = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<ErrorOr<PaginatedResult<UserStoryDto>>>;
