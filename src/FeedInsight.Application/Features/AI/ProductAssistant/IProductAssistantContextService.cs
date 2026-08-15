using FeedInsight.Application.Common.Models;

namespace FeedInsight.Application.Features.AI.ProductAssistant;

public interface IProductAssistantContextService
{
    Task<string> GetRelevantContextAsync(
        Guid tenantId,
        string userQuestion,
        CancellationToken cancellationToken = default);
}