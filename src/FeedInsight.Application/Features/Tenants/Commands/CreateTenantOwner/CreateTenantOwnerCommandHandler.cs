using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Tenants.Commands.CreateTenantOwner;

public class CreateTenantOwnerCommandHandler : IRequestHandler<CreateTenantOwnerCommand, ErrorOr<Guid>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenantOwnerCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> HandleAsync(CreateTenantOwnerCommand request, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null)
        {
            return Errors.Auth.Unauthenticated;
        }

        var currentUser = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (currentUser is null) return Errors.Users.NotFound;

        if (currentUser.TenantId is null)
        {
            return Errors.Users.NotAssociatedWithTenant;
        }

        bool emailExists = await _userRepository.AnyAsync(new UserByEmailSpec(request.Email), cancellationToken);
        if (emailExists)
        {
            return Errors.Users.DuplicateEmail;
        }

        var newUser = new User(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            _passwordHasher,
            tenantId: currentUser.TenantId);

        var productOwnerRole = await _roleRepository.SingleOrDefaultAsync(new RoleByNameSpec(Role.ProductOwner), cancellationToken);
        if (productOwnerRole is null)
        {
            return Errors.Users.RoleNotFound;
        }

        newUser.AssignRole(productOwnerRole.Id);

        await _userRepository.AddAsync(newUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newUser.Id;
    }
}
