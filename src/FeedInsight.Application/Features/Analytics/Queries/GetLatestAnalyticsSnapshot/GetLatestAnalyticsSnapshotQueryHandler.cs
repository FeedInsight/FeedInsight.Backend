using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Analytics.DTOs;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Analytics.Queries.GetLatestAnalyticsSnapshot;

public class GetLatestAnalyticsSnapshotQueryHandler: IRequestHandler< GetLatestAnalyticsSnapshotQuery,ErrorOr<AnalyticsSnapshotDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IAnalyticsSnapshotService _analyticsSnapshotService;

    public GetLatestAnalyticsSnapshotQueryHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IAnalyticsSnapshotService analyticsSnapshotService)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _analyticsSnapshotService = analyticsSnapshotService;
    }

    public async Task<ErrorOr<AnalyticsSnapshotDto>> HandleAsync(
        GetLatestAnalyticsSnapshotQuery request,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.UserId is null)
            return Errors.Auth.Unauthenticated;

        var user = await _userRepository.GetByIdAsync(
            _currentUserService.UserId.Value,
            cancellationToken);

        if (user is null || user.TenantId is null)
            return Errors.Users.NotAssociatedWithTenant;

        var snapshotDate =
            DateOnly.FromDateTime(DateTime.UtcNow);

        var analytics =
            await _analyticsSnapshotService.GenerateSnapshotAsync(
                user.TenantId.Value,
                snapshotDate,
                cancellationToken);

        return new AnalyticsSnapshotDto(
            analytics.SnapshotDate,
            analytics.TotalFeedbacksReceived,
            analytics.PositiveSentimentCount,
            analytics.NeutralSentimentCount,
            analytics.NegativeSentimentCount,
            analytics.TotalTasksExtracted,
            analytics.DraftTicketsGenerated,
            analytics.PoApprovalRatePercent,
            analytics.TopRequestedFeaturesJson);
    }
}