using ErrorOr;
using FeedInsight.Application.Features.Analytics.DTOs;

using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Analytics.Queries.GetLatestAnalyticsSnapshot;

public record GetLatestAnalyticsSnapshotQuery
    : IRequest<ErrorOr<AnalyticsSnapshotDto>>;