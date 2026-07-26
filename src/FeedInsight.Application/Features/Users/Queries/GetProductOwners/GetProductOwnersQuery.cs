using ErrorOr;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Users.Queries.GetProductOwners;

public record GetProductOwnersQuery(
    string? SearchTerm,
    Guid? TenantId,
    int Page = 1,
    int PageSize = 10
) : IRequest<ErrorOr<PaginatedResult<ProductOwnerDto>>>;
