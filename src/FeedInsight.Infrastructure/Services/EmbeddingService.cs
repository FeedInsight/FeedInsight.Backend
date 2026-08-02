using FeedInsight.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using OpenAI;
using System.ClientModel;

namespace FeedInsight.Infrastructure.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly ITextEmbeddingGenerationService _embeddingGenerator;

    public EmbeddingService(IConfiguration configuration)
    {
        var apiKey = configuration["HuggingFace:ApiKey"];
        var modelId = configuration["HuggingFace:EmbeddingModelId"] ?? "qwen3-embedding-8b";
        var endpointUrl = configuration["HuggingFace:EmbeddingEndpoint"] ?? "https://router.huggingface.co/scaleway/v1/";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("HuggingFace API Key is missing from configuration.");
        }

        // Configure OpenAI Client to point to the Hugging Face Router
        var options = new OpenAIClientOptions { Endpoint = new Uri(endpointUrl) };
        var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey), options);

        // Initialize the embedding service using the compatible client
#pragma warning disable CS0618 // Type or member is obsolete
        _embeddingGenerator = new OpenAITextEmbeddingGenerationService(
            modelId: modelId,
            openAIClient: openAiClient);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var embeddings = await _embeddingGenerator.GenerateEmbeddingsAsync(new List<string> { text }, cancellationToken: cancellationToken);
        return embeddings.FirstOrDefault();
    }
}
