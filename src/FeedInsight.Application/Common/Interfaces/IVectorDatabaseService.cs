using FeedInsight.Domain.ExtractedTasks;

namespace FeedInsight.Application.Common.Interfaces;

public interface IVectorDatabaseService
{
    Task UpsertTaskAsync(ExtractedTask task, ReadOnlyMemory<float> embedding, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<VectorSearchResult>> SearchTasksAsync(
        ReadOnlyMemory<float> queryEmbedding, 
        int limit = 5, 
        Guid? tenantId = null, 
        CancellationToken cancellationToken = default);
}

public record VectorSearchResult(Guid TaskId, float Score, string Text, Guid TenantId, Guid? CategoryId);
