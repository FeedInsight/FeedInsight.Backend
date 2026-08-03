using System.Text;
using ErrorOr;
using FeedInsight.Application.Common.Constants;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Application.Features.AI.ProductAssistant;
using FeedInsight.Application.Features.AI.ProductAssistant.Models;
using FeedInsight.Application.Features.ChatAssistant.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Chats;
using FeedInsight.Domain.Chats.Enums;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;

namespace FeedInsight.Application.Features.ChatAssistant.Queries.AskProductAssistant;

public class AskProductAssistantQueryHandler
    : IRequestHandler<AskProductAssistantQuery, ErrorOr<string>>
{
    private const int SearchResultLimit = 18;

    private readonly IRepository<ChatSession> _sessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantResolver _tenantResolver;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDatabaseService _vectorDatabaseService;
    private readonly IProductAssistantService _productAssistantService;

    public AskProductAssistantQueryHandler(
        IRepository<ChatSession> sessionRepository,
        ICurrentUserService currentUserService,
        ITenantResolver tenantResolver,
        IEmbeddingService embeddingService,
        IVectorDatabaseService vectorDatabaseService,
        IProductAssistantService productAssistantService)
    {
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
        _tenantResolver = tenantResolver;
        _embeddingService = embeddingService;
        _vectorDatabaseService = vectorDatabaseService;
        _productAssistantService = productAssistantService;
    }

    public async Task<ErrorOr<string>> HandleAsync(
        AskProductAssistantQuery request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated ||
            !_currentUserService.UserId.HasValue)
        {
            return Errors.Auth.Unauthenticated;
        }

        var tenantId = await _tenantResolver.ResolveTenantIdAsync(cancellationToken);

        if (!tenantId.HasValue)
        {
            return Errors.Tenants.NotFound;
        }

        var session = await _sessionRepository.FirstOrDefaultAsync(
            new ChatSessionByIdSpec(
                tenantId.Value,
                _currentUserService.UserId.Value,
                request.SessionId),
            cancellationToken);

        if (session is null)
        {
            return Errors.Chat.SessionNotFound;
        }

        var embedding = await _embeddingService.GenerateEmbeddingAsync(
            request.UserQuestion,
            cancellationToken);

        var tenantFilter = new MetadataFilter
        {
            MustMatch = new Dictionary<string, object>
            {
                { "TenantId", tenantId.Value.ToString() }
            }
        };

        var extractedTaskResults = await _vectorDatabaseService.SearchAsync<ExtractedTaskPayload>(
            VectorCollectionNames.ExtractedTasks,
            embedding,
            SearchResultLimit,
            tenantFilter,
            cancellationToken);

        var userStoryResults = await _vectorDatabaseService.SearchAsync<UserStoryPayload>(
            VectorCollectionNames.UserStories,
            embedding,
            SearchResultLimit,
            tenantFilter,
            cancellationToken);

        var mergedResults = extractedTaskResults
            .Select(r => (Score: r.Score, Context: FormatExtractedTaskContext(r)))
            .Concat(userStoryResults.Select(r => (Score: r.Score, Context: FormatUserStoryContext(r))))
            .OrderByDescending(r => r.Score)
            .Take(SearchResultLimit)
            .Select(r => r.Context)
            .ToList();

        var databaseContext = mergedResults.Count > 0
            ? string.Join("\n\n", mergedResults)
            : "No relevant backlog items were found for this query.";

        var conversationHistory = BuildConversationHistory(session.Messages, request.UserQuestion);

        var answer = await _productAssistantService.GenerateAnswerAsync(
            request.UserQuestion,
            databaseContext,
            conversationHistory,
            cancellationToken);

        return answer;
    }

    private static IReadOnlyList<ChatHistoryMessage> BuildConversationHistory(
        IReadOnlyCollection<ChatMessage> messages,
        string currentQuestion)
    {
        var priorMessages = messages
            .Where(m => m.SenderRole != ChatRole.System)
            .OrderBy(m => m.CreatedAt)
            .ToList();

        if (priorMessages.Count > 0 &&
            priorMessages[^1].SenderRole == ChatRole.User &&
            priorMessages[^1].Content == currentQuestion)
        {
            priorMessages = priorMessages.Take(priorMessages.Count - 1).ToList();
        }

        return priorMessages
            .Select(m => new ChatHistoryMessage(m.SenderRole.ToString(), m.Content))
            .ToList();
    }

    private static string FormatExtractedTaskContext(VectorSearchResult<ExtractedTaskPayload> result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[ExtractedTask | ID: {result.PointId} | Score: {result.Score:F4}]");
        sb.AppendLine(result.Payload.Text);
        return sb.ToString().TrimEnd();
    }

    private static string FormatUserStoryContext(VectorSearchResult<UserStoryPayload> result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[UserStory | ID: {result.PointId} | Title: {result.Payload.Title} | Jira: {result.Payload.JiraTicketKey ?? "N/A"} | Status: {result.Payload.Status} | Urgency: {result.Payload.UrgencyScore} | Score: {result.Score:F4}]");
        sb.AppendLine(result.Payload.Text);
        return sb.ToString().TrimEnd();
    }
}
