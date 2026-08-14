using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.CustomerFeedbacks.Development.Dtos;
using FeedInsight.Application.Features.CustomerFeedbacks.Development.Specifications;
using FeedInsight.Application.Features.CustomerFeedbacks.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.Users;
using static FeedInsight.Application.Features.CustomerFeedbacks.Development.Queries.GetCompanyCustomerFeedbacks.GetCompanyCustomerFeedbacksQueryHandler;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Development.Queries.GetCompanyCustomerFeedbacks;

public class GetCompanyCustomerFeedbacksQueryHandler : IRequestHandler<GetCompanyCustomerFeedbacksQuery, ErrorOr<PaginatedResult<CompanyCustomerFeedbackDto>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<CustomerFeedback> _feedbackRepository;
    private readonly IRepository<FeedbackComment> _commentRepository;

    public GetCompanyCustomerFeedbacksQueryHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<CustomerFeedback> feedbackRepository,
        IRepository<FeedbackComment> commentRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _feedbackRepository = feedbackRepository;
        _commentRepository = commentRepository;
    }

    public async Task<ErrorOr<PaginatedResult<CompanyCustomerFeedbackDto>>> HandleAsync(
        GetCompanyCustomerFeedbacksQuery request,
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

        var tenantId = user.TenantId.Value;

        // 3. Get total count
        var totalCount = await _feedbackRepository.CountAsync(
            new CustomerFeedbacksByTenantSpec(tenantId),
            cancellationToken);

        // 4. Get paginated feedbacks
        var feedbacks = await _feedbackRepository.ListAsync(
            new CustomerFeedbacksByTenantSpec(
                tenantId,
                request.Page,
                request.PageSize),
            cancellationToken);

        if (!feedbacks.Any())
        {
            return new PaginatedResult<CompanyCustomerFeedbackDto>(
                new List<CompanyCustomerFeedbackDto>(),
                totalCount);
        }

        // 5. Get all feedback IDs
        var feedbackIds = feedbacks
            .Select(f => f.Id)
            .ToList();

        // 6. Get comments for all feedbacks in one query
        var comments = await _commentRepository.ListAsync(
            new FeedbackCommentsByFeedbackIdsSpec(feedbackIds),
            cancellationToken);

        // 7. Group comments by feedback
        var commentsByFeedback = comments
            .GroupBy(c => c.CustomerFeedbackId)
            .ToDictionary(
                g => g.Key,
                g => g.ToList());

        // 8. Build response
        var items = feedbacks
            .Select(f => new CompanyCustomerFeedbackDto(
                f.Id,
                f.RawContent,
                f.SubmitterEmail,
                f.MetadataJson,
                f.OverallSentiment,
                f.IsProcessedByRouter,
                f.CreatedAt,
                commentsByFeedback.TryGetValue(
                    f.Id,
                    out var feedbackComments)
                    ? feedbackComments
                        .Select(c => new FeedbackCommentDto(
                            c.Id,
                            c.UserId,
                            c.Content,
                            c.CreatedAt))
                        .ToList()
                    : new List<FeedbackCommentDto>()
            ))
            .ToList();

        return new PaginatedResult<CompanyCustomerFeedbackDto>(
            items,
            totalCount);
    }

    public record CompanyCustomerFeedbackDto(
    Guid Id,
    string RawContent,
    string? SubmitterEmail,
    string? MetadataJson,
    string? OverallSentiment,
    bool IsProcessedByRouter,
    DateTime CreatedAt,
    List<FeedbackCommentDto> Comments
);
}