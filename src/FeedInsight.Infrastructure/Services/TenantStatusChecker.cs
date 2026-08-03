using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Tenants.Enums;

namespace FeedInsight.Infrastructure.Services;

public class TenantStatusChecker : ITenantStatusChecker
{
    private readonly IRepository<Tenant> _tenantRepository;

    public TenantStatusChecker(IRepository<Tenant> tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<ErrorOr<Success>> EnsureActiveAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(
            tenantId,
            cancellationToken);

        if (tenant is null)
        {
            return Errors.Tenants.NotFound;
        }

        if (tenant.Status != TenantStatus.Active)
        {
            return Errors.Auth.TenantNotActive;
        }

        return Result.Success;
    }
}