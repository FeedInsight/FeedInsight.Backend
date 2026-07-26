using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ErrorOr<Success>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> HandleAsync(LogoutCommand request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.SingleOrDefaultAsync(new UserByRefreshTokenSpec(request.RefreshToken), cancellationToken);

        if (user is null)
        {
            return Result.Success;
        }

        user.RevokeRefreshToken(request.RefreshToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
