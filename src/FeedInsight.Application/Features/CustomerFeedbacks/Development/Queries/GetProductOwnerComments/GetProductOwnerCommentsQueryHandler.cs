using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.CustomerFeedbacks.Development.Dtos;
using FeedInsight.Application.Features.CustomerFeedbacks.Development.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Development.Queries.GetProductOwnerComments
{
    public class GetProductOwnerCommentsQueryHandler
     : IRequestHandler<
         GetProductOwnerCommentsQuery,
         ErrorOr<PaginatedResult<CustomerFeedbackWithProductOwnerCommentsDto>>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<CustomerFeedback> _feedbackRepository;
        private readonly IRepository<FeedbackComment> _commentRepository;

        public GetProductOwnerCommentsQueryHandler(
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

        public async Task<
            ErrorOr<PaginatedResult<CustomerFeedbackWithProductOwnerCommentsDto>>>
            HandleAsync(
                GetProductOwnerCommentsQuery request,
                CancellationToken cancellationToken = default)
        {
            // 1. Get authenticated user
            if (_currentUserService.UserId is null)
            {
                return Errors.Auth.Unauthenticated;
            }

            // 2. Get user
            var user = await _userRepository.GetByIdAsync(
                _currentUserService.UserId.Value,
                cancellationToken);

            if (user is null || user.TenantId is null)
            {
                return Errors.Users.NotAssociatedWithTenant;
            }

            var tenantId = user.TenantId.Value;
            var userId = user.Id;

            // 3. Get total count of THIS customer's feedbacks
            var totalCount = await _feedbackRepository.CountAsync(
                new CustomerFeedbacksBySubmitterCountSpec(
                    tenantId,
                    userId),
                cancellationToken);

            // 4. Get THIS customer's feedbacks
            var feedbacks = await _feedbackRepository.ListAsync(
                new CustomerFeedbacksBySubmitterSpec(
                    tenantId,
                    userId,
                    request.Page,
                    request.PageSize),
                cancellationToken);

            if (!feedbacks.Any())
            {
                return new PaginatedResult<
                    CustomerFeedbackWithProductOwnerCommentsDto>(
                    new List<CustomerFeedbackWithProductOwnerCommentsDto>(),
                    totalCount);
            }

            // 5. Get feedback IDs
            var feedbackIds = feedbacks
                .Select(f => f.Id)
                .ToList();

            // 6. Get comments for these feedbacks
            var comments = await _commentRepository.ListAsync(
                new ProductOwnerCommentsByFeedbackIdsSpec(feedbackIds),
                cancellationToken);

            // 7. Group comments by feedback
            var commentsByFeedback = comments
                .GroupBy(c => c.CustomerFeedbackId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList());

            // 8. Build response
            var items = feedbacks
                .Select(f => new CustomerFeedbackWithProductOwnerCommentsDto(
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
                            .Select(c => new ProductOwnerCommentDto(
                                c.Id,
                                c.UserId,
                                c.Content,
                                c.CreatedAt))
                            .ToList()
                        : new List<ProductOwnerCommentDto>()
                ))
                .ToList();

            return new PaginatedResult<
                CustomerFeedbackWithProductOwnerCommentsDto>(
                items,
                totalCount);
        }
    }
}
