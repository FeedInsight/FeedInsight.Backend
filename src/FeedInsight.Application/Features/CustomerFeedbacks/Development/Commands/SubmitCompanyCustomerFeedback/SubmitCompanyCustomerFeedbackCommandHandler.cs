using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Users.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Development.Commands.SubmitCompanyCustomerFeedback;

public class SubmitCompanyCustomerFeedbackCommandHandler
    : IRequestHandler<SubmitCompanyCustomerFeedbackCommand, ErrorOr<Guid>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<CustomerFeedback> _feedbackRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitCompanyCustomerFeedbackCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<CustomerFeedback> feedbackRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _feedbackRepository = feedbackRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> HandleAsync(
        SubmitCompanyCustomerFeedbackCommand request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;

        if (!userId.HasValue)
        {
            return Errors.Auth.Unauthenticated;
        }

        var user = await _userRepository.FirstOrDefaultAsync(
            new UserByIdSpec(userId.Value),
            cancellationToken);

        if (user is null)
        {
            return Errors.Auth.Unauthenticated;
        }

        if (!user.TenantId.HasValue)
        {
            return Errors.Auth.Unauthenticated;
        }

        var feedback = new CustomerFeedback(
            user.TenantId.Value,
            request.RawContent,
            metadataJson: request.MetadataJson,
            submitterUserId: user.Id);

        await _feedbackRepository.AddAsync(
            feedback,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return feedback.Id;
    }
}