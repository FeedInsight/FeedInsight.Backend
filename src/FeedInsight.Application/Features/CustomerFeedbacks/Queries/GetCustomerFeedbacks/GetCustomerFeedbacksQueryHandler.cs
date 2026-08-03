using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.CustomerFeedbacks.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.ExtractedTasks;
using FeedInsight.Domain.Users;
using System.Linq;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Queries.GetCustomerFeedbacks;

public class GetCustomerFeedbacksQueryHandler : IRequestHandler<GetCustomerFeedbacksQuery, ErrorOr<PaginatedResult<CustomerFeedbackDto>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<CustomerFeedback> _feedbackRepository;
    private readonly IRepository<ExtractedTask> _extractedTaskRepository;

    public GetCustomerFeedbacksQueryHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<CustomerFeedback> feedbackRepository,
        IRepository<ExtractedTask> extractedTaskRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _feedbackRepository = feedbackRepository;
        _extractedTaskRepository = extractedTaskRepository;
    }

    public async Task<ErrorOr<PaginatedResult<CustomerFeedbackDto>>> HandleAsync(GetCustomerFeedbacksQuery request, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null) return Errors.Auth.Unauthenticated;

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null || user.TenantId is null) return Errors.Users.NotAssociatedWithTenant;

        var tenantId = user.TenantId.Value;

        // Get total count of feedbacks
        var totalCount = await _feedbackRepository.CountAsync(new CustomerFeedbacksByTenantSpec(tenantId), cancellationToken);

        // Get paginated feedbacks
        var feedbacks = await _feedbackRepository.ListAsync(new CustomerFeedbacksByTenantSpec(tenantId, request.Page, request.PageSize), cancellationToken);

        if (!feedbacks.Any())
        {
            return new PaginatedResult<CustomerFeedbackDto>(new List<CustomerFeedbackDto>(), totalCount);
        }

        var feedbackIds = feedbacks.Select(f => f.Id).ToList();

        // Get extracted tasks for these feedbacks
        var extractedTasks = await _extractedTaskRepository.ListAsync(new ExtractedTasksByFeedbackIdsSpec(feedbackIds), cancellationToken);
        var tasksByFeedback = extractedTasks.GroupBy(t => t.CustomerFeedbackId).ToDictionary(g => g.Key, g => g.ToList());

        var items = feedbacks.Select(f => new CustomerFeedbackDto(
            f.Id,
            f.RawContent,
            f.SubmitterEmail,
            f.MetadataJson,
            f.OverallSentiment,
            f.IsProcessedByRouter,
            f.CreatedAt,
            tasksByFeedback.TryGetValue(f.Id, out var tasks) ? tasks.Select(t => new ExtractedTaskDto(
                t.Id,
                t.CategoryId,
                t.UserStoryId,
                t.ExtractedIntent,
                t.TechnicalKeywords,
                t.SyncStatus.ToString(),
                t.JiraSubtaskKey,
                t.CreatedAt
            )).ToList() : new List<ExtractedTaskDto>()
        )).ToList();

        return new PaginatedResult<CustomerFeedbackDto>(items, totalCount);
    }
}
