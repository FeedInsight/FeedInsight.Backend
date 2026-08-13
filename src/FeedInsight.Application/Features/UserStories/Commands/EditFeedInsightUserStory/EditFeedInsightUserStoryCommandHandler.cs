using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace FeedInsight.Application.Features.UserStories.Commands.EditFeedInsightUserStory;

public class EditFeedInsightUserStoryCommandHandler : IRequestHandler<EditFeedInsightUserStoryCommand, ErrorOr<Success>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserStory> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public EditFeedInsightUserStoryCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<UserStory> repository, 
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> HandleAsync(EditFeedInsightUserStoryCommand request, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null) return Errors.Auth.Unauthenticated;

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null || user.TenantId is null) return Errors.Users.NotAssociatedWithTenant;

        var story = await _repository.GetByIdAsync(request.UserStoryId, cancellationToken);

        if (story == null || story.TenantId != user.TenantId.Value)
        {
            return Errors.UserStories.NotFound;
        }

        if (story.Source != UserStorySource.FeedInsight)
        {
            return Errors.UserStories.InvalidSource;
        }

        story.UpdateDetails(request.Title, request.AcceptanceCriteria);

        await _repository.UpdateAsync(story, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
