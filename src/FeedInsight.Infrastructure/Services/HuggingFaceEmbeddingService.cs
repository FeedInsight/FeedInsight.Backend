using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FeedInsight.Application.Common.Interfaces;
using Microsoft.Extensions.AI;

namespace FeedInsight.Infrastructure.Services;

public class HuggingFaceEmbeddingService : IEmbeddingService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    public HuggingFaceEmbeddingService(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator;
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var embeddings = await _embeddingGenerator.GenerateAsync(new List<string> { text }, cancellationToken: cancellationToken);
        var firstEmbedding = embeddings.FirstOrDefault();
        return firstEmbedding != null ? firstEmbedding.Vector : ReadOnlyMemory<float>.Empty;
    }
}
