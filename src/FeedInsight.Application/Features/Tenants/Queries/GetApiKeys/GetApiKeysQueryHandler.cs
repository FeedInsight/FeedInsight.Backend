using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Tenants.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Tenants.Queries.GetApiKeys;

public class GetApiKeysQueryHandler : IRequestHandler<GetApiKeysQuery, ErrorOr<List<ApiKeyDto>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Tenant> _tenantRepository;

    public GetApiKeysQueryHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<Tenant> tenantRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task<ErrorOr<List<ApiKeyDto>>> HandleAsync(GetApiKeysQuery request, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null)
        {
            return Errors.Auth.Unauthenticated;
        }

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null) return Errors.Users.NotFound;

        if (user.TenantId is null)
        {
            return Errors.Users.NotAssociatedWithTenant;
        }

        var spec = new TenantWithApiKeysSpec(user.TenantId.Value);
        var tenant = await _tenantRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (tenant is null) return Errors.Tenants.NotFound;

        var dtos = tenant.ApiKeys
            .Where(k => !k.IsDeleted)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new ApiKeyDto(k.Id, k.Name, k.Prefix, k.ExpiresAt, k.IsActive, k.CreatedAt))
            .ToList();

        return dtos;
    }
}
