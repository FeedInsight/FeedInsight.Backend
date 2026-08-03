using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ErrorOr<Success>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> HandleAsync(UpdateProfileCommand request, CancellationToken cancellationToken = default)
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

        user.UpdateProfile(request.FirstName, request.LastName);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
