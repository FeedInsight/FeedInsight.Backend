using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Ingestion.Specifications;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Tenants;
using System.Security.Claims;

namespace FeedInsight.API.Services;

public class TenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentApiKeyService _currentApiKeyService;
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IApiKeyHasher _apiKeyHasher;

    public TenantResolver(
        IHttpContextAccessor httpContextAccessor,
        ICurrentApiKeyService currentApiKeyService,
        IRepository<Tenant> tenantRepository,
        IApiKeyHasher apiKeyHasher)
    {
        _httpContextAccessor = httpContextAccessor;
        _currentApiKeyService = currentApiKeyService;
        _tenantRepository = tenantRepository;
        _apiKeyHasher = apiKeyHasher;
    }

    public async Task<Guid?> ResolveTenantIdAsync(
        CancellationToken cancellationToken = default)
    {
        // 1. Authenticated Tenant User → resolve from JWT
        var tenantIdClaim = _httpContextAccessor.HttpContext?
            .User?
            .FindFirst("tenantId")?
            .Value;

        if (Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return tenantId;
        }

        // 2. Public / Ingestion → resolve from X-Api-Key
        var apiKey = _currentApiKeyService.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var keyHash = _apiKeyHasher.Hash(apiKey);

        var spec = new TenantByApiKeyHashSpec(keyHash);

        var tenant = await _tenantRepository.FirstOrDefaultAsync(
            spec,
            cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        var apiKeyEntity = tenant.ApiKeys
            .FirstOrDefault(k => k.KeyHash == keyHash);

        if (apiKeyEntity is null || !apiKeyEntity.IsActive)
        {
            return null;
        }

        return tenant.Id;
    }
}