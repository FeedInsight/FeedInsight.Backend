using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace FeedInsight.Infrastructure.Services;

/// <summary>
/// Round-robin pooled embedding generator. Falls back to the next API key
/// automatically on 402 Payment Required (quota exhausted).
/// </summary>
public sealed class PooledEmbeddingGenerator : DelegatingEmbeddingGenerator<string, Embedding<float>>
{
    private readonly IReadOnlyList<IEmbeddingGenerator<string, Embedding<float>>> _generators;
    private readonly ILogger<PooledEmbeddingGenerator> _logger;
    private volatile int _currentIndex = 0;

    public PooledEmbeddingGenerator(
        IReadOnlyList<IEmbeddingGenerator<string, Embedding<float>>> generators,
        ILogger<PooledEmbeddingGenerator> logger)
        : base(generators[0])
    {
        if (generators == null || generators.Count == 0)
            throw new ArgumentException("At least one generator must be provided.", nameof(generators));

        _generators = generators;
        _logger = logger;
    }

    public override async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        int startIndex = _currentIndex;

        for (int attempt = 0; attempt < _generators.Count; attempt++)
        {
            int index = (startIndex + attempt) % _generators.Count;
            try
            {
                var result = await _generators[index].GenerateAsync(values, options, cancellationToken);
                _currentIndex = index;
                return result;
            }
            catch (ClientResultException ex) when (ex.Status == 402)
            {
                _logger.LogWarning(
                    "Embedding API key [{Index}] quota exhausted (402). Trying next key... ({Remaining} left)",
                    index, _generators.Count - attempt - 1);
                _currentIndex = (index + 1) % _generators.Count;
            }
        }

        throw new InvalidOperationException(
            $"All {_generators.Count} embedding API key(s) exhausted (402). Add more keys to HuggingFace:EmbeddingApiKeys.");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var g in _generators)
                try { g.Dispose(); } catch { }
        }
        base.Dispose(disposing);
    }
}