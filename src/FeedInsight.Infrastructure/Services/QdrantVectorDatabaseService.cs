using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Domain.ExtractedTasks;
using Microsoft.Extensions.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace FeedInsight.Infrastructure.Services;

public class QdrantVectorDatabaseService : IVectorDatabaseService
{
    private readonly QdrantClient _client;
    private const string CollectionName = "extracted_tasks";
    private const ulong VectorSize = 4096; // qwen3-embedding-8b outputs 4096 dimensions

    public QdrantVectorDatabaseService(IConfiguration configuration)
    {
        var url = configuration["Qdrant:Url"];
        var apiKey = configuration["Qdrant:ApiKey"];

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Qdrant configuration is missing.");
        }

        var uri = new Uri(url);

        // Qdrant Cloud gRPC usually uses port 6334.
        var port = uri.IsDefaultPort ? 6334 : uri.Port;

        // Initialize official Qdrant gRPC client
        _client = new QdrantClient(
            host: uri.Host,
            port: port,
            https: uri.Scheme == "https",
            apiKey: apiKey
        );
    }

    public async Task UpsertTaskAsync(ExtractedTask task, ReadOnlyMemory<float> embedding, CancellationToken cancellationToken = default)
    {
        // 1. Ensure collection exists
        try
        {
            await _client.GetCollectionInfoAsync(CollectionName, cancellationToken);
        }
        catch
        {
            // Collection doesn't exist, create it
            await _client.CreateCollectionAsync(
                collectionName: CollectionName,
                vectorsConfig: new VectorParams { Size = VectorSize, Distance = Distance.Cosine },
                cancellationToken: cancellationToken
            );

            // Qdrant requires an index to filter by fields.
            await _client.CreatePayloadIndexAsync(
                CollectionName,
                "tenantId",
                PayloadSchemaType.Keyword,
                cancellationToken: cancellationToken
            );
        }

        var text = $"{task.ExtractedIntent} {task.TechnicalKeywords}".Trim();

        // 2. Prepare Point payload
        var point = new PointStruct
        {
            Id = task.Id, // Supports Guid natively!
            Vectors = embedding.ToArray(),
            Payload =
            {
                ["tenantId"] = task.TenantId.ToString(),
                ["categoryId"] = task.CategoryId.ToString(),
                ["text"] = text
            }
        };

        // 3. Upsert
        await _client.UpsertAsync(CollectionName, new[] { point }, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchTasksAsync(
        ReadOnlyMemory<float> queryEmbedding,
        int limit = 5,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        Filter? filter = null;

        if (tenantId.HasValue)
        {
            filter = new Filter
            {
                Must = {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "tenantId",
                            Match = new Match { Keyword = tenantId.Value.ToString() }
                        }
                    }
                }
            };
        }

        var results = await _client.SearchAsync(
            collectionName: CollectionName,
            vector: queryEmbedding.ToArray(),
            filter: filter,
            limit: (ulong)limit,
            payloadSelector: true,
            cancellationToken: cancellationToken
        );

        var searchResults = new List<VectorSearchResult>();

        foreach (var result in results)
        {
            var payload = result.Payload;
            var taskId = Guid.Parse(result.Id.Uuid);

            var tenantStr = payload.TryGetValue("tenantId", out var tId) ? tId.StringValue : null;
            var categoryStr = payload.TryGetValue("categoryId", out var cId) ? cId.StringValue : null;
            var text = payload.TryGetValue("text", out var txt) ? txt.StringValue : string.Empty;

            var parsedTenant = Guid.TryParse(tenantStr, out var parsedT) ? parsedT : Guid.Empty;
            Guid? parsedCategory = Guid.TryParse(categoryStr, out var parsedC) ? parsedC : null;

            searchResults.Add(new VectorSearchResult(
                taskId,
                result.Score,
                text,
                parsedTenant,
                parsedCategory));
        }

        return searchResults;
    }
}
