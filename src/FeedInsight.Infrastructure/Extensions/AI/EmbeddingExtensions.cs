using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Infrastructure.Options;
using FeedInsight.Infrastructure.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace FeedInsight.Infrastructure.Extensions.AI;

public static class EmbeddingExtensions
{
    public static IServiceCollection AddEmbeddings(this IServiceCollection services, IConfiguration configuration)
    {
#pragma warning disable SKEXP0010 // Type or member is obsolete
        var hfSettings = configuration.GetSection(HuggingFaceSettings.SectionName).Get<HuggingFaceSettings>();

        if (hfSettings != null && hfSettings.EmbeddingApiKeys.Length > 0)
        {
            var generators = hfSettings.EmbeddingApiKeys
                .Select(key =>
                {
                    var clientOptions = new OpenAIClientOptions { Endpoint = new Uri(hfSettings.EmbeddingEndpoint) };
                    var client = new OpenAIClient(new ApiKeyCredential(key), clientOptions);
                    return (IEmbeddingGenerator<string, Embedding<float>>)
                        client.GetEmbeddingClient(hfSettings.EmbeddingModelId).AsIEmbeddingGenerator();
                })
                .ToList();

            // Singleton so the round-robin index is shared across all requests.
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
                new PooledEmbeddingGenerator(generators, sp.GetRequiredService<ILogger<PooledEmbeddingGenerator>>()));
        }
#pragma warning restore SKEXP0010 // Type or member is obsolete

        services.AddScoped<IEmbeddingService, HuggingFaceEmbeddingService>();

        return services;
    }
}
