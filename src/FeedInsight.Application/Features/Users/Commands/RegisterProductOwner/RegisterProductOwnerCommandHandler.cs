using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Commands.RegisterProductOwner;

public class RegisterProductOwnerCommandHandler : IRequestHandler<RegisterProductOwnerCommand, ErrorOr<Guid>>
{
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterProductOwnerCommandHandler(
        IRepository<Tenant> tenantRepository,
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<Category> categoryRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _categoryRepository = categoryRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> HandleAsync(RegisterProductOwnerCommand request, CancellationToken cancellationToken = default)
    {
        bool emailExists = await _userRepository.AnyAsync(new UserByEmailSpec(request.Email), cancellationToken);
        if (emailExists)
        {
            return Errors.Users.DuplicateEmail;
        }

        var tenant = new Tenant(request.CompanyName);
        await _tenantRepository.AddAsync(tenant, cancellationToken);

        var defaultCategory = new Category(
            tenantId: tenant.Id,
            name: "Uncategorized",
            description: "Use this category ONLY if the feedback does not fit into any other available category.",
            isSystemDefault: true
        );
        await _categoryRepository.AddAsync(defaultCategory, cancellationToken);

        var user = new User(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            _passwordHasher,
            tenantId: tenant.Id);

        var productOwnerRole = await _roleRepository.SingleOrDefaultAsync(new RoleByNameSpec(Role.ProductOwner), cancellationToken);
        if (productOwnerRole is null)
        {
            return Errors.Users.RoleNotFound;
        }

        user.AssignRole(productOwnerRole.Id);
        await _userRepository.AddAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}