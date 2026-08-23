using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Queries.GetProfile;

public class GetProfileQueryHandler: IRequestHandler<GetProfileQuery, ErrorOr<ProfileDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Tenant> _tenantRepository;

    public GetProfileQueryHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<Tenant> tenantRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task<ErrorOr<ProfileDto>> HandleAsync(
        GetProfileQuery request,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null)
            return Errors.Auth.Unauthenticated;

        var user = await _userRepository.GetByIdAsync(
            _currentUserService.UserId.Value,
            cancellationToken);

        if (user is null)
            return Errors.Users.NotFound;

        if (user.TenantId is null)
            return Errors.Users.NotAssociatedWithTenant;

        var tenant = await _tenantRepository.GetByIdAsync(
            user.TenantId.Value,
            cancellationToken);

        if (tenant is null)
            return Errors.Tenants.NotFound;

        return new ProfileDto(
            user.FirstName,
            user.LastName,
            tenant.CompanyName);
    }
}