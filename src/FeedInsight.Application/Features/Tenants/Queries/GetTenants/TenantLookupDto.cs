namespace FeedInsight.Application.Features.Tenants.Queries.GetTenants;

public record TenantLookupDto(
    Guid Id,
    string CompanyName
);