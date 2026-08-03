using FeedInsight.Domain.Tenants.Enums;

namespace FeedInsight.Application.Features.Tenants.Queries.GetTenants;

public record TenantLookupDto(
    Guid Id,
    string CompanyName,
    string Status,
    DateTime CreatedAt
);