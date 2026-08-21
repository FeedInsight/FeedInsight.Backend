using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.CompanyCustomers.Commands.DeleteCompanyCustomer;
using FeedInsight.Application.Features.Customers.Specifications;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Tenants.Enums;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Customers.Commands.DeleteCompanyCustomer;

public class DeleteCompanyCustomerCommandHandler
    : IRequestHandler<DeleteCompanyCustomerCommand, ErrorOr<Success>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCompanyCustomerCommandHandler(
        IRepository<User> userRepository,
        IRepository<Tenant> tenantRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> HandleAsync(
        DeleteCompanyCustomerCommand request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
            return Errors.Auth.Unauthenticated;

        var currentUser = await _userRepository.FirstOrDefaultAsync(
            new UserByIdSpec(currentUserId.Value),
            cancellationToken);

        if (currentUser is null || currentUser.TenantId is null)
            return Errors.Auth.Unauthenticated;

        if (!currentUser.HasRole(Role.ProductOwner))
            return Errors.Auth.Unauthenticated;

        var tenant = await _tenantRepository.GetByIdAsync(
            currentUser.TenantId.Value,
            cancellationToken);

        if (tenant is null)
            return Errors.Tenants.NotFound;

        if (tenant.CompanyType != CompanyType.Development)
            return Errors.Users.RoleNotFound;

        var customer = await _userRepository.FirstOrDefaultAsync(
            new CompanyCustomerByIdSpec(
                request.CustomerId,
                currentUser.TenantId.Value),
            cancellationToken);

        if (customer is null)
            return Errors.Users.NotFound;

        customer.SoftDelete();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}