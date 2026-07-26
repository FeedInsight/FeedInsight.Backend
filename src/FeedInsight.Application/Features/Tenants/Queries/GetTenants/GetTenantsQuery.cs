using ErrorOr;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.Users.Queries.GetProductOwners;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Tenants.Queries.GetTenants;

public record GetTenantsQuery(
    string? SearchTerm,
    int? Page = null,
    int? PageSize = null
) : IRequest<ErrorOr<PaginatedResult<TenantLookupDto>>>;