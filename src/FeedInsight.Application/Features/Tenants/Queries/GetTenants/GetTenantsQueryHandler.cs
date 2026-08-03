using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.Tenants.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Tenants;

namespace FeedInsight.Application.Features.Tenants.Queries.GetTenants;

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, ErrorOr<PaginatedResult<TenantLookupDto>>>
{
    private readonly IRepository<Tenant> _tenantRepository;

    public GetTenantsQueryHandler(IRepository<Tenant> tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    public async Task<ErrorOr<PaginatedResult<TenantLookupDto>>> HandleAsync(GetTenantsQuery request, CancellationToken cancellationToken = default)
    {
        var spec = new TenantLookupSpec(
            request.SearchTerm,
            request.Page,
            request.PageSize);

        var tenants = await _tenantRepository.ListAsync(spec, cancellationToken);

        var dto = tenants
            .Select(t => new TenantLookupDto(
                t.Id,
        t.CompanyName,
        t.Status.ToString(),
        t.CreatedAt))
            .ToList();

        var totalCount = await _tenantRepository.CountAsync(
            new TenantLookupCountSpec(request.SearchTerm),
            cancellationToken);

        return new PaginatedResult<TenantLookupDto>(dto, totalCount);
    }
}
