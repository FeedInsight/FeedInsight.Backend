using Ardalis.Specification;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.Analytics.Queries.Specifications;
using FeedInsight.Application.Features.CustomerFeedbacks.Specifications;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.ExtractedTasks;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Enums;
using System.Text.Json;
using static FeedInsight.Application.Common.Interfaces.IAnalyticsSnapshotService;

namespace FeedInsight.Infrastructure.Services;

public class AnalyticsSnapshotService : IAnalyticsSnapshotService
{
    private readonly IRepository<CustomerFeedback> _feedbackRepository;
    private readonly IRepository<ExtractedTask> _taskRepository;
    private readonly IRepository<UserStory> _userStoryRepository;
    private readonly IRepository<Category> _categoryRepository;

    public AnalyticsSnapshotService(
        IRepository<CustomerFeedback> feedbackRepository,
        IRepository<ExtractedTask> taskRepository,
        IRepository<UserStory> userStoryRepository,
        IRepository<Category> categoryRepository)
    {
        _feedbackRepository = feedbackRepository;
        _taskRepository = taskRepository;
        _userStoryRepository = userStoryRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<DailyAnalyticsSnapshotDto> GenerateSnapshotAsync(
     Guid tenantId,
     DateOnly snapshotDate,
     CancellationToken cancellationToken = default)
    {
        var snapshotEndDate = snapshotDate
            .AddDays(1)
            .ToDateTime(TimeOnly.MinValue);

        var feedbacks = await _feedbackRepository.ListAsync(
            new FeedbacksByTenantAndDateSpec(
                tenantId,
                snapshotEndDate),
            cancellationToken);

        var totalFeedbacksReceived = feedbacks.Count;

        var positiveSentimentCount = feedbacks.Count(x =>
            string.Equals(
                x.OverallSentiment,
                "Positive",
                StringComparison.OrdinalIgnoreCase));

        var neutralSentimentCount = feedbacks.Count(x =>
            string.Equals(
                x.OverallSentiment,
                "Neutral",
                StringComparison.OrdinalIgnoreCase));

        var negativeSentimentCount = feedbacks.Count(x =>
            string.Equals(
                x.OverallSentiment,
                "Negative",
                StringComparison.OrdinalIgnoreCase));

        var tasks = await _taskRepository.ListAsync(
            new ExtractedTasksByTenantAndDateSpec(
                tenantId,
                snapshotEndDate),
            cancellationToken);

        var totalTasksExtracted = tasks.Count;

        var userStories = await _userStoryRepository.ListAsync(
            new UserStoriesByTenantAndDateSpec(
                tenantId,
                snapshotEndDate),
            cancellationToken);

        var feedInsightStories = userStories
            .Where(x => x.Source == UserStorySource.FeedInsight)
            .ToList();

        var draftTicketsGenerated = feedInsightStories.Count;

        var approvedStories = feedInsightStories.Count(x =>
            x.Status == UserStoryStatus.Synced);

        var poApprovalRatePercent =
            feedInsightStories.Count == 0
                ? 0
                : Math.Round(
                    (decimal)approvedStories /
                    feedInsightStories.Count *
                    100,
                    2);

        var categoryIds = tasks
            .Select(x => x.CategoryId)
            .Distinct()
            .ToList();

        var categories = categoryIds.Count == 0
            ? []
            : await _categoryRepository.ListAsync(
                new CategoriesByIdsSpec(categoryIds),
                cancellationToken);

        var categoryLookup = categories.ToDictionary(
            x => x.Id,
            x => x.Name);

        var topRequestedFeatures = tasks
            .GroupBy(x => x.CategoryId)
            .Select(group => new
            {
                Feature = categoryLookup.TryGetValue(
                    group.Key,
                    out var categoryName)
                    ? categoryName
                    : "Unknown",

                Count = group.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        var topRequestedFeaturesJson =
            JsonSerializer.Serialize(topRequestedFeatures);

        return new DailyAnalyticsSnapshotDto(
            SnapshotDate: snapshotDate,
            TotalFeedbacksReceived: totalFeedbacksReceived,
            PositiveSentimentCount: positiveSentimentCount,
            NeutralSentimentCount: neutralSentimentCount,
            NegativeSentimentCount: negativeSentimentCount,
            TotalTasksExtracted: totalTasksExtracted,
            DraftTicketsGenerated: draftTicketsGenerated,
            PoApprovalRatePercent: poApprovalRatePercent,
            TopRequestedFeaturesJson: topRequestedFeaturesJson);
    }





}