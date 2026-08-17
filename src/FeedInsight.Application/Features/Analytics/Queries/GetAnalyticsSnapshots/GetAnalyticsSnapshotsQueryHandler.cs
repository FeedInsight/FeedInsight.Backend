using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.Analytics.DTOs;

using FeedInsight.Application.Features.Analytics.Queries.Specifications;

using FeedInsight.Application.Messaging;

using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.DailyAnalyticsSnapshot;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Analytics.Queries.GetAnalyticsSnapshots;

public class GetAnalyticsSnapshotsQueryHandler
    : IRequestHandler<
        GetAnalyticsSnapshotsQuery,
        ErrorOr<PaginatedResult<AnalyticsSnapshotDto>>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<DailyAnalyticsSnapshot> _snapshotRepository;

    public GetAnalyticsSnapshotsQueryHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<DailyAnalyticsSnapshot> snapshotRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _snapshotRepository = snapshotRepository;
    }

    public async Task<ErrorOr<PaginatedResult<AnalyticsSnapshotDto>>> HandleAsync(
        GetAnalyticsSnapshotsQuery request,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null)
            return Errors.Auth.Unauthenticated;

        var user = await _userRepository.GetByIdAsync(
            _currentUserService.UserId.Value,
            cancellationToken);

        if (user is null || user.TenantId is null)
            return Errors.Users.NotAssociatedWithTenant;

        var spec = new AnalyticsSnapshotsByDateRangeSpec(
            user.TenantId.Value,
            request.From,
            request.To);

        var snapshots = await _snapshotRepository.ListAsync(
            spec,
            cancellationToken);

        var totalCount = snapshots.Count;

        var items = snapshots
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(snapshot => new AnalyticsSnapshotDto(
                snapshot.SnapshotDate,
                snapshot.TotalFeedbacksReceived,
                snapshot.PositiveSentimentCount,
                snapshot.NeutralSentimentCount,
                snapshot.NegativeSentimentCount,
                snapshot.TotalTasksExtracted,
                snapshot.DraftTicketsGenerated,
                snapshot.PoApprovalRatePercent,
                snapshot.TopRequestedFeaturesJson))
            .ToList();

        return new PaginatedResult<AnalyticsSnapshotDto>(
            items,
            totalCount);
    }
}