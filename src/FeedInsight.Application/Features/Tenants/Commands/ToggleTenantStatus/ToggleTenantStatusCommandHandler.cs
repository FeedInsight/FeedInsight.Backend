using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Tenants.Enums;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Tenants.Commands.ToggleTenantStatus;

public class ToggleTenantStatusCommandHandler
    : IRequestHandler<ToggleTenantStatusCommand, ErrorOr<Success>>
{
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleTenantStatusCommandHandler(
        IRepository<Tenant> tenantRepository,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> HandleAsync(
        ToggleTenantStatusCommand request,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(
            request.TenantId,
            cancellationToken);

        if (tenant is null)
        {
            return Errors.Tenants.NotFound;
        }

        switch (request.Status)
        {
            case TenantStatus.Active:
                tenant.Activate();
                break;

            case TenantStatus.Suspended:
                tenant.Suspend(request.Reason!);
                break;

            case TenantStatus.Deactivated:
                tenant.Deactivate(request.Reason!);
                break;
        }

        if (request.Status is TenantStatus.Suspended or TenantStatus.Deactivated)
        {
            var users = await _userRepository.ListAsync(
                new UsersByTenantIdSpec(request.TenantId),
                cancellationToken);

            foreach (var user in users)
            {
                user.RevokeAllRefreshTokens();
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}