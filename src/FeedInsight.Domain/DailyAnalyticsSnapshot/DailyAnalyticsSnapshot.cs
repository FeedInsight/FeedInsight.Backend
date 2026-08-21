using FeedInsight.Domain.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Domain.DailyAnalyticsSnapshot
{
    public class DailyAnalyticsSnapshot : Entity
    {
        public Guid TenantId { get; private set; }

        public DateOnly SnapshotDate { get; private set; }

        public int TotalFeedbacksReceived { get; private set; }

        public int PositiveSentimentCount { get; private set; }

        public int NeutralSentimentCount { get; private set; }

        public int NegativeSentimentCount { get; private set; }

        public int TotalTasksExtracted { get; private set; }

        public int DraftTicketsGenerated { get; private set; }

        public decimal PoApprovalRatePercent { get; private set; }

        public string TopRequestedFeaturesJson { get; private set; } = "[]";

        private DailyAnalyticsSnapshot()
        {
        }

        public DailyAnalyticsSnapshot(
            Guid tenantId,
            DateOnly snapshotDate,
            int totalFeedbacksReceived,
            int positiveSentimentCount,
            int neutralSentimentCount,
            int negativeSentimentCount,
            int totalTasksExtracted,
            int draftTicketsGenerated,
            decimal poApprovalRatePercent,
            string? topRequestedFeaturesJson = null)
        {
            TenantId = tenantId;
            SnapshotDate = snapshotDate;
            TotalFeedbacksReceived = totalFeedbacksReceived;
            PositiveSentimentCount = positiveSentimentCount;
            NeutralSentimentCount = neutralSentimentCount;
            NegativeSentimentCount = negativeSentimentCount;
            TotalTasksExtracted = totalTasksExtracted;
            DraftTicketsGenerated = draftTicketsGenerated;
            PoApprovalRatePercent = poApprovalRatePercent;
            TopRequestedFeaturesJson =
                string.IsNullOrWhiteSpace(topRequestedFeaturesJson)
                    ? "[]"
                    : topRequestedFeaturesJson;
        }
    }
    }
