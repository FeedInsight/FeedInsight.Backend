using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.UserStories.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Users;
using FeedInsight.Domain.UserStories;

namespace FeedInsight.Application.Features.UserStories.Queries.GetUserStories;

public class GetUserStoriesQueryHandler : IRequestHandler<GetUserStoriesQuery, ErrorOr<PaginatedResult<UserStoryDto>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserStory> _repository;

    public GetUserStoriesQueryHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<UserStory> repository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _repository = repository;
    }

    public async Task<ErrorOr<PaginatedResult<UserStoryDto>>> HandleAsync(GetUserStoriesQuery request, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null) return Errors.Auth.Unauthenticated;

        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null || user.TenantId is null) return Errors.Users.NotAssociatedWithTenant;

        var countSpec = new UserStoriesByFiltersSpec(
            user.TenantId.Value,
            request.Source,
            request.IsSynced,
            request.SearchTerm,
            request.CategoryId);

        var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

        var pagedSpec = new UserStoriesByFiltersSpec(
            user.TenantId.Value,
            request.Source,
            request.IsSynced,
            request.SearchTerm,
            request.CategoryId,
            request.PageNumber,
            request.PageSize);

        var stories = await _repository.ListAsync(pagedSpec, cancellationToken);

        var dtos = stories.Select(s => new UserStoryDto(
            s.Id,
            s.TenantId,
            s.CategoryId,
            s.Category?.Name,
            s.Source,
            s.JiraTicketKey,
            s.Title,
            s.AcceptanceCriteria,
            s.UrgencyScore,
            s.Status
        )).ToList();

        return new PaginatedResult<UserStoryDto>(dtos, totalCount);
    }
}
