using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Analytics.DTOs
{
    public record AnalyticsSnapshotDto(
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
