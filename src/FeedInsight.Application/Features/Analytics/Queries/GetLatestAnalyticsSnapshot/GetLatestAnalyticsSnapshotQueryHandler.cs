using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Analytics.DTOs;

using FeedInsight.Application.Features.Analytics.Queries.Specifications;

using FeedInsight.Application.Messaging;

using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.DailyAnalyticsSnapshot;
using FeedInsight.Domain.Users;

namespace FeedInsight.Application.Features.Analytics.Queries.GetLatestAnalyticsSnapshot;

public class GetLatestAnalyticsSnapshotQueryHandler
    : IRequestHandler<
        GetLatestAnalyticsSnapshotQuery,
        ErrorOr<AnalyticsSnapshotDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<DailyAnalyticsSnapshot> _snapshotRepository;

    public GetLatestAnalyticsSnapshotQueryHandler(
        ICurrentUserService currentUserService,
        IRepository<User> userRepository,
        IRepository<DailyAnalyticsSnapshot> snapshotRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _snapshotRepository = snapshotRepository;
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

        var snapshot =
            await _snapshotRepository.SingleOrDefaultAsync(
                new LatestAnalyticsSnapshotSpec(user.TenantId.Value),
                cancellationToken);

      

        return new AnalyticsSnapshotDto(
            snapshot.SnapshotDate,
            snapshot.TotalFeedbacksReceived,
            snapshot.PositiveSentimentCount,
            snapshot.NeutralSentimentCount,
            snapshot.NegativeSentimentCount,
            snapshot.TotalTasksExtracted,
            snapshot.DraftTicketsGenerated,
            snapshot.PoApprovalRatePercent,
            snapshot.TopRequestedFeaturesJson);
    }
}