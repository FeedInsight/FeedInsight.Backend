using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Common.Interfaces
{
    public interface IAnalyticsSnapshotService
    {
        Task<DailyAnalyticsSnapshotDto> GenerateSnapshotAsync(
        Guid tenantId,
        DateOnly snapshotDate,
        CancellationToken cancellationToken = default);
        public record DailyAnalyticsSnapshotDto(
    DateOnly SnapshotDate,
    int TotalFeedbacksReceived,
    int PositiveSentimentCount,
    int NeutralSentimentCount,
    int NegativeSentimentCount,
    int TotalTasksExtracted,
    int DraftTicketsGenerated,
    decimal PoApprovalRatePercent,
    string TopRequestedFeaturesJson);
    }
}
