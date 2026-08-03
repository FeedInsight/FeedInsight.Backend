using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ErrorOr<Success>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }
    public async Task<ErrorOr<Success>> HandleAsync(ChangePasswordCommand request, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null)
        {
            return Errors.Auth.Unauthenticated;
        }

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null)
        {
            return Errors.Users.NotFound;
        }

        if (!user.VerifyPassword(request.CurrentPassword, _passwordHasher))
        {
            return Errors.Users.IncorrectPassword;
        }

        user.ChangePassword(request.NewPassword, _passwordHasher);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
