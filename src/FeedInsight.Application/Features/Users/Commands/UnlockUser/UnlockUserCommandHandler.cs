using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Commands.UnlockUser;

public class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand, ErrorOr<Success>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnlockUserCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> HandleAsync(UnlockUserCommand request, CancellationToken cancellationToken = default)
    {
        var targetUser = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (targetUser is null)
        {
            return Errors.Users.NotFound;
        }

        targetUser.UnlockAccount();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
