using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Tenants.Enums;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Customers.Commands.CreateCompanyCustomer;

public class CreateCompanyCustomerCommandHandler : IRequestHandler<CreateCompanyCustomerCommand, ErrorOr<Guid>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateCompanyCustomerCommandHandler(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<Guid>> HandleAsync(CreateCompanyCustomerCommand request,CancellationToken cancellationToken = default)
    {
        // 1. Get currently authenticated user
        var currentUserId = _currentUserService.UserId;

        if (currentUserId is null)
        {
            return Errors.Auth.InvalidCredentials;
        }

        // 2. Get current user + Tenant
        var currentUser = await _userRepository.FirstOrDefaultAsync(
            new UserByIdSpec(currentUserId.Value),
            cancellationToken);

        if (currentUser is null || currentUser.TenantId is null)
        {
            return Errors.Auth.InvalidCredentials;
        }

       

        var emailExists = await _userRepository.AnyAsync( new UserByEmailSpec(request.Email), cancellationToken);

        if (emailExists)
        {
            return Errors.Users.DuplicateEmail;
        }

      
        var customerRole = await _roleRepository.SingleOrDefaultAsync(
            new RoleByNameSpec(Role.CompanyCustomer),
            cancellationToken);

        if (customerRole is null)
        {
            return Errors.Users.RoleNotFound;
        }

       
        var customer = new User(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            _passwordHasher,
            currentUser.TenantId);

       
        customer.AssignRole(customerRole.Id);

      
        await _userRepository.AddAsync(customer, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}