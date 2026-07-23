using ErrorOr;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Feeds.Queries.GetFeeds;

/// <summary>
/// The Request object that holds the parameters for fetching a list of feeds.
/// </summary>
public record GetFeedsQuery(
    string? SearchTerm,
    int Page = 1,
    int PageSize = 10
) : IRequest<ErrorOr<PaginatedResult<FeedDto>>>;