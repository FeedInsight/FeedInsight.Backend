using System;
using System.ClientModel;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Infrastructure.Options;
using FeedInsight.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace FeedInsight.Infrastructure.Extensions.AI;

public static class EmbeddingExtensions
{
    public static IServiceCollection AddEmbeddings(this IServiceCollection services, IConfiguration configuration)
    {
#pragma warning disable SKEXP0010 // Type or member is obsolete
        var hfSettingsConfig = configuration.GetSection(HuggingFaceSettings.SectionName).Get<HuggingFaceSettings>();
        if (hfSettingsConfig != null && !string.IsNullOrWhiteSpace(hfSettingsConfig.ApiKey))
        {
            var clientOptions = new OpenAIClientOptions { Endpoint = new Uri(hfSettingsConfig.EmbeddingEndpoint) };
            var openAiClient = new OpenAIClient(new ApiKeyCredential(hfSettingsConfig.ApiKey), clientOptions);
            services.AddOpenAIEmbeddingGenerator(hfSettingsConfig.EmbeddingModelId, openAiClient);
        }
#pragma warning restore SKEXP0010 // Type or member is obsolete

        services.AddScoped<IEmbeddingService, HuggingFaceEmbeddingService>();

        return services;
    }
}
