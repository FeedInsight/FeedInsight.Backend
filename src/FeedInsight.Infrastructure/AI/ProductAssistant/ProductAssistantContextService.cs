using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.AI.ProductAssistant;
using FeedInsight.Application.Features.UserStories.Specifications;
using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.ExtractedTasks;
using FeedInsight.Domain.UserStories;
using Microsoft.Extensions.Logging;
using System.Text;

namespace FeedInsight.Infrastructure.AI.ProductAssistant;

public class ProductAssistantContextService : IProductAssistantContextService
{
    private const string UserStoriesCollection = "user_stories";
    private const string ExtractedTasksCollection = "extracted_tasks";

    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDatabaseService _qdrantService;

    private readonly IRepository<CustomerFeedback> _feedbackRepository;
    private readonly IRepository<ExtractedTask> _taskRepository;
    private readonly IRepository<UserStory> _userStoryRepository;

    private readonly ILogger<ProductAssistantContextService> _logger;

    public ProductAssistantContextService(
        IEmbeddingService embeddingService,
        IVectorDatabaseService qdrantService,
        IRepository<CustomerFeedback> feedbackRepository,
        IRepository<ExtractedTask> taskRepository,
        IRepository<UserStory> userStoryRepository,
        ILogger<ProductAssistantContextService> logger)
    {
        _embeddingService = embeddingService;
        _qdrantService = qdrantService;
        _feedbackRepository = feedbackRepository;
        _taskRepository = taskRepository;
        _userStoryRepository = userStoryRepository;
        _logger = logger;
    }

    public async Task<string> GetRelevantContextAsync(
        Guid tenantId,
        string userQuestion,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
            return string.Empty;

        /*
         * Ranking / aggregation questions should not depend only
         * on semantic similarity.
         *
         * Example:
         * "What is the most urgent story?"
         *
         * We need the stories ordered by UrgencyScore,
         * not just the stories semantically closest to the word "urgent".
         */
        if (IsUrgencyQuestion(userQuestion))
        {
            return await GetUrgencyContextAsync(
                tenantId,
                cancellationToken);
        }

        var questionEmbedding =
            await _embeddingService.GenerateEmbeddingAsync(
                userQuestion,
                cancellationToken);

        if (questionEmbedding.IsEmpty)
        {
            _logger.LogWarning(
                "Could not generate embedding for Product Assistant question.");

            return string.Empty;
        }

        var filter = new MetadataFilter
        {
            MustMatch = new Dictionary<string, object>
            {
                ["TenantId"] = tenantId
            }
        };

        // Get more than 5 because the assistant may need
        // several related records to answer naturally.
        const int searchLimit = 15;

        var userStoryResults =
            await _qdrantService.SearchAsync<UserStoryPayload>(
                UserStoriesCollection,
                questionEmbedding,
                limit: searchLimit,
                filter: filter,
                cancellationToken: cancellationToken);

        var taskResults =
            await _qdrantService.SearchAsync<ExtractedTaskPayload>(
                ExtractedTasksCollection,
                questionEmbedding,
                limit: searchLimit,
                filter: filter,
                cancellationToken: cancellationToken);

        var context = new StringBuilder();

        AppendUserStories(
            context,
            userStoryResults);

        await AppendTasksAndFeedback(
            context,
            taskResults,
            tenantId,
            cancellationToken);

        return context.ToString();
    }

    private async Task<string> GetUrgencyContextAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        /*
         * Get the actual stories from SQL ordered by urgency.
         * This is much more reliable than semantic search for ranking questions.
         */
        var stories = await _userStoryRepository.ListAsync(
            new UserStoriesByFiltersSpec(
                tenantId,
                source: null,
                isSynced: null,
                searchTerm: null,
                categoryId: null),
            cancellationToken);

        var orderedStories = stories
            .OrderByDescending(x => x.UrgencyScore)
            .Take(10)
            .ToList();

        if (!orderedStories.Any())
            return string.Empty;

        var context = new StringBuilder();

        context.AppendLine("USER STORIES");
        context.AppendLine();

        foreach (var story in orderedStories)
        {
            context.AppendLine($"Title: {story.Title}");
            context.AppendLine($"Jira Ticket: {story.JiraTicketKey ?? "N/A"}");
            context.AppendLine($"Status: {story.Status}");
            context.AppendLine($"Urgency Score: {story.UrgencyScore}");

            if (!string.IsNullOrWhiteSpace(story.AcceptanceCriteria))
            {
                context.AppendLine(
                    $"Acceptance Criteria: {story.AcceptanceCriteria}");
            }

            context.AppendLine();
        }

        return context.ToString();
    }

    private static void AppendUserStories(
        StringBuilder context,
        IReadOnlyList<VectorSearchResult<UserStoryPayload>> results)
    {
        if (!results.Any())
            return;

        context.AppendLine("USER STORIES");
        context.AppendLine();

        foreach (var result in results)
        {
            var story = result.Payload;

            context.AppendLine($"Title: {story.Title}");
            context.AppendLine($"Jira Ticket: {story.JiraTicketKey ?? "N/A"}");
            context.AppendLine($"Status: {story.Status}");
            context.AppendLine($"Urgency Score: {story.UrgencyScore}");

            if (!string.IsNullOrWhiteSpace(story.AcceptanceCriteria))
            {
                context.AppendLine(
                    $"Acceptance Criteria: {story.AcceptanceCriteria}");
            }

            context.AppendLine();
        }
    }

    private async Task AppendTasksAndFeedback(
        StringBuilder context,
        IReadOnlyList<VectorSearchResult<ExtractedTaskPayload>> results,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!results.Any())
            return;

        context.AppendLine("CUSTOMER FEEDBACK AND EXTRACTED TASKS");
        context.AppendLine();

        foreach (var result in results)
        {
            var extractedTask =
                await _taskRepository.GetByIdAsync(
                    result.PointId,
                    cancellationToken);

            if (extractedTask is null ||
                extractedTask.TenantId != tenantId)
            {
                continue;
            }

            var feedback =
                await _feedbackRepository.GetByIdAsync(
                    extractedTask.CustomerFeedbackId,
                    cancellationToken);

            context.AppendLine(
                $"Problem: {extractedTask.ExtractedIntent}");

            if (!string.IsNullOrWhiteSpace(
                    extractedTask.TechnicalKeywords))
            {
                context.AppendLine(
                    $"Keywords: {extractedTask.TechnicalKeywords}");
            }

            if (feedback is not null)
            {
                context.AppendLine(
                    $"Customer Feedback: {feedback.RawContent}");

                if (!string.IsNullOrWhiteSpace(
                        feedback.OverallSentiment))
                {
                    context.AppendLine(
                        $"Sentiment: {feedback.OverallSentiment}");
                }
            }

            context.AppendLine();
        }
    }

    private static bool IsUrgencyQuestion(string question)
    {
        var normalized = question.ToLowerInvariant();

        return normalized.Contains("urgent")
               || normalized.Contains("urgency")
               || normalized.Contains("highest priority")
               || normalized.Contains("most important")
               || normalized.Contains("priority");
    }
}