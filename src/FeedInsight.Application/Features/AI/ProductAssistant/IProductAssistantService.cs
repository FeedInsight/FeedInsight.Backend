using FeedInsight.Application.Features.AI.ProductAssistant.Models;

namespace FeedInsight.Application.Features.AI.ProductAssistant;

public interface IProductAssistantService
{
    Task<string> GenerateAnswerAsync(
        string userQuestion,
        string databaseContext,
        IReadOnlyList<ChatHistoryMessage> conversationHistory,
        CancellationToken cancellationToken = default);
}
