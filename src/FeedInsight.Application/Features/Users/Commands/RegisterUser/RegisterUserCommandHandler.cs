using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ErrorOr<Guid>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> HandleAsync(RegisterUserCommand request, CancellationToken cancellationToken = default)
    {
        bool emailExists = await _userRepository.AnyAsync(new UserByEmailSpec(request.Email), cancellationToken);
        if (emailExists)
        {
            return Errors.Users.DuplicateEmail;
        }

        var user = new User(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            _passwordHasher
        );

        var superAdminRole = await _roleRepository.SingleOrDefaultAsync(new RoleByNameSpec(Role.SuperAdmin), cancellationToken);

        if (superAdminRole is null)
        {
            return Errors.Users.RoleNotFound;
        }

        user.AssignRole(superAdminRole.Id);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
