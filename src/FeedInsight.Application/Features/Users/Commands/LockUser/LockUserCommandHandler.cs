using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Commands.LockUser;

public class LockUserCommandHandler : IRequestHandler<LockUserCommand, ErrorOr<Success>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LockUserCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> HandleAsync(LockUserCommand request, CancellationToken cancellationToken = default)
    {
        var targetUser = await _userRepository.SingleOrDefaultAsync(new UserByIdSpec(request.UserId), cancellationToken);

        if (targetUser is null)
        {
            return Errors.Users.NotFound;
        }

        if (targetUser.HasRole(Role.SuperAdmin))
        {
            return Errors.Users.CannotLockSuperAdmin;
        }

        targetUser.LockAccount(request.Reason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
