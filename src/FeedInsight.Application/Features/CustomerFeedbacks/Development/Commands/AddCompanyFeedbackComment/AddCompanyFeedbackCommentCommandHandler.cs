using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Commands.Development.AddCompanyFeedbackComment;

public class AddCompanyFeedbackCommentCommandHandler
    : IRequestHandler<AddCompanyFeedbackCommentCommand, ErrorOr<Guid>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<CustomerFeedback> _feedbackRepository;
    private readonly IRepository<FeedbackComment> _commentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCompanyFeedbackCommentCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<CustomerFeedback> feedbackRepository,
        IRepository<FeedbackComment> commentRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _feedbackRepository = feedbackRepository;
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> HandleAsync(
        AddCompanyFeedbackCommentCommand request,
        CancellationToken cancellationToken = default)
    {
        // 1. Get current user
        if (_currentUserService.UserId is null)
        {
            return Errors.Auth.Unauthenticated;
        }

        // 2. Get current user + Tenant
        var user = await _userRepository.GetByIdAsync(
            _currentUserService.UserId.Value,
            cancellationToken);

        if (user is null || user.TenantId is null)
        {
            return Errors.Users.NotAssociatedWithTenant;
        }

        // 3. Get feedback
        var feedback = await _feedbackRepository.GetByIdAsync(
            request.FeedbackId,
            cancellationToken);

       

        // 5. Create comment
        var comment = new FeedbackComment(
            feedback.Id,
            user.Id,
            request.Content);

        // 6. Save comment
        await _commentRepository.AddAsync(
            comment,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Return created comment ID
        return comment.Id;
    }
}