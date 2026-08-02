using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Models;
using FeedInsight.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace FeedInsight.Infrastructure.Services;

public class QdrantVectorDatabaseService : IVectorDatabaseService
{
    private readonly QdrantClient _client;
    private readonly QdrantSettings _settings;

    public QdrantVectorDatabaseService(QdrantClient client, IOptions<QdrantSettings> options)
    {
        _client = client;
        _settings = options.Value;
    }

    public async Task UpsertPointAsync<TPayload>(string collectionName, Guid pointId, ReadOnlyMemory<float> embedding, TPayload payload, CancellationToken cancellationToken = default)
    {
        await EnsureCollectionExistsAsync(collectionName, cancellationToken);

        var point = new PointStruct
        {
            Id = pointId,
            Vectors = embedding.ToArray()
        };

        var mappedPayload = MapPayload(payload);
        foreach (var kvp in mappedPayload)
        {
            point.Payload.Add(kvp.Key, kvp.Value);
        }

        await _client.UpsertAsync(collectionName, new[] { point }, cancellationToken: cancellationToken);
    }

    public async Task UpsertPointsAsync<TPayload>(string collectionName, IReadOnlyList<VectorPoint<TPayload>> points, CancellationToken cancellationToken = default)
    {
        await EnsureCollectionExistsAsync(collectionName, cancellationToken);

        var pointStructs = new List<PointStruct>();
        foreach (var p in points)
        {
            var point = new PointStruct
            {
                Id = p.Id,
                Vectors = p.Embedding.ToArray()
            };

            var mappedPayload = MapPayload(p.Payload);
            foreach (var kvp in mappedPayload)
            {
                point.Payload.Add(kvp.Key, kvp.Value);
            }
            pointStructs.Add(point);
        }

        await _client.UpsertAsync(collectionName, pointStructs, cancellationToken: cancellationToken);
    }

    public async Task DeletePointAsync(string collectionName, Guid pointId, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync(collectionName, new[] { (PointId)pointId }, cancellationToken: cancellationToken);
    }

    public async Task DeletePointsAsync(string collectionName, IReadOnlyList<Guid> pointIds, CancellationToken cancellationToken = default)
    {
        var ids = pointIds.Select(id => (PointId)id).ToList();
        await _client.DeleteAsync(collectionName, ids, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<VectorSearchResult<TPayload>>> SearchAsync<TPayload>(
        string collectionName,
        ReadOnlyMemory<float> queryEmbedding,
        int limit = 5,
        MetadataFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        Filter? qdrantFilter = null;

        if (filter != null && (filter.MustMatch?.Any() == true || filter.MustNotMatch?.Any() == true))
        {
            qdrantFilter = new Filter();

            if (filter.MustMatch != null)
            {
                foreach (var match in filter.MustMatch)
                {
                    qdrantFilter.Must.Add(new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = match.Key,
                            Match = new Match { Keyword = match.Value?.ToString() ?? string.Empty }
                        }
                    });
                }
            }

            if (filter.MustNotMatch != null)
            {
                foreach (var match in filter.MustNotMatch)
                {
                    qdrantFilter.MustNot.Add(new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = match.Key,
                            Match = new Match { Keyword = match.Value?.ToString() ?? string.Empty }
                        }
                    });
                }
            }
        }

        var results = await _client.SearchAsync(
            collectionName: collectionName,
            vector: queryEmbedding.ToArray(),
            filter: qdrantFilter,
            limit: (ulong)limit,
            payloadSelector: true,
            cancellationToken: cancellationToken
        );

        var searchResults = new List<VectorSearchResult<TPayload>>();

        foreach (var result in results)
        {
            var taskId = Guid.Parse(result.Id.Uuid);
            var mappedPayload = MapToPayload<TPayload>(result.Payload);

            if (mappedPayload != null)
            {
                searchResults.Add(new VectorSearchResult<TPayload>(
                    taskId,
                    result.Score,
                    mappedPayload));
            }
        }

        return searchResults;
    }


    private async Task EnsureCollectionExistsAsync(string collectionName, CancellationToken cancellationToken)
    {
        try
        {
            await _client.GetCollectionInfoAsync(collectionName, cancellationToken);
        }
        catch
        {
            // Collection doesn't exist, create it.
            // Look up the config, or use a safe default if not found
            var hasConfig = _settings.Collections.TryGetValue(collectionName, out var config);
            var vectorSize = hasConfig ? config.VectorSize : 4096; // fallback size
            
            Distance distanceMetric = Distance.Cosine;
            if (hasConfig && Enum.TryParse<Distance>(config.Distance, true, out var parsedDistance))
            {
                distanceMetric = parsedDistance;
            }

            await _client.CreateCollectionAsync(
                collectionName: collectionName,
                vectorsConfig: new VectorParams { Size = vectorSize, Distance = distanceMetric },
                cancellationToken: cancellationToken
            );

            // Create configured indexes
            if (hasConfig && config.IndexFields != null)
            {
                foreach (var field in config.IndexFields)
                {
                    await _client.CreatePayloadIndexAsync(
                        collectionName,
                        field,
                        PayloadSchemaType.Keyword,
                        cancellationToken: cancellationToken
                    );
                }
            }
        }
    }

    // Helper to map complex objects to Qdrant's Payload map
    private Dictionary<string, Value> MapPayload<TPayload>(TPayload payload)
    {
        var dict = new Dictionary<string, Value>();
        if (payload == null) return dict;

        var jsonElement = JsonSerializer.SerializeToElement(payload);
        foreach (var prop in jsonElement.EnumerateObject())
        {
            dict[prop.Name] = MapJsonElementToValue(prop.Value);
        }
        return dict;
    }

    private Value MapJsonElementToValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.TryGetInt64(out var l) ? (Value)l : (Value)element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => new Value { NullValue = Qdrant.Client.Grpc.NullValue.NullValue },
            _ => element.GetRawText()
        };
    }

    // Helper to map Qdrant's Payload map back to complex objects
    private TPayload? MapToPayload<TPayload>(IReadOnlyDictionary<string, Value> payloadMap)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var kvp in payloadMap)
        {
            dict[kvp.Key] = kvp.Value.KindCase switch
            {
                Value.KindOneofCase.StringValue => kvp.Value.StringValue,
                Value.KindOneofCase.IntegerValue => kvp.Value.IntegerValue,
                Value.KindOneofCase.DoubleValue => kvp.Value.DoubleValue,
                Value.KindOneofCase.BoolValue => kvp.Value.BoolValue,
                _ => null
            };
        }
        var json = JsonSerializer.Serialize(dict);
        return JsonSerializer.Deserialize<TPayload>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
