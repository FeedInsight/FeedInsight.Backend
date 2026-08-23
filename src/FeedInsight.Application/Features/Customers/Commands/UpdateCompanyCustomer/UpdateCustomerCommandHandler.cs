using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Customers.Commands.UpdateCompanyCustomer;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;

public class UpdateCustomerCommandHandler
    : IRequestHandler<UpdateCustomerCommand, ErrorOr<Success>>
{
    private readonly IRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(
        IRepository<User> userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> HandleAsync(
        UpdateCustomerCommand request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;

        if (!currentUserId.HasValue)
            return Errors.Auth.Unauthenticated;

        var currentUser = await _userRepository
            .FirstOrDefaultAsync(
                new UserByIdSpec(currentUserId.Value),
                cancellationToken);

        if (currentUser is null || !currentUser.TenantId.HasValue)
            return Errors.Auth.Unauthenticated;

        var customer = await _userRepository
            .FirstOrDefaultAsync(
                new UserByIdSpec(request.CustomerId),
                cancellationToken);

        if (customer is null)
            return Errors.Users.NotFound;

        if (customer.TenantId != currentUser.TenantId)
            return Errors.Users.NotFound;

        if (!customer.HasRole(Role.CompanyCustomer))
            return Errors.Users.RoleNotFound;

        customer.UpdateProfile(
            request.FirstName,
            request.LastName);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}