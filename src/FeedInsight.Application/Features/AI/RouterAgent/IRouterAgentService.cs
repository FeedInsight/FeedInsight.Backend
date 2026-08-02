using FeedInsight.Application.Features.AI.RouterAgent.Models;
using FeedInsight.Domain.Categories;

namespace FeedInsight.Application.Features.AI.RouterAgent;

/// <summary>
/// Abstract contract for the AI Router Agent.
/// </summary>
public interface IRouterAgentService
{
    /// <summary>
    /// Analyzes raw customer feedback, splits it into multiple technical intents if necessary, 
    /// and assigns each intent to the most accurate category.
    /// </summary>
    Task<RouterAgentResponse> ProcessFeedbackAsync(
        string rawFeedback,
        IEnumerable<Category> availableCategories,
        CancellationToken cancellationToken = default);
}
