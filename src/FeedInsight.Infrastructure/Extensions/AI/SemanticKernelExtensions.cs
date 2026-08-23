using System;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.AI.RouterAgent;
using FeedInsight.Application.Features.AI.TriageAgent;
using FeedInsight.Infrastructure.AI.RouterAgent;
using FeedInsight.Infrastructure.AI.TriageAgent;
using FeedInsight.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace FeedInsight.Infrastructure.Extensions.AI;

public static class SemanticKernelExtensions
{
    public static IServiceCollection AddSemanticKernelAgents(this IServiceCollection services)
    {
        services.AddScoped<IRouterAgentService, RouterAgentService>();
        services.AddScoped<ITriageAgentService, TriageAgentService>();

        services.AddTransient<Kernel>(sp =>
        {
            var builder = Kernel.CreateBuilder();
            var hfSettings = sp.GetRequiredService<IOptions<HuggingFaceSettings>>().Value;

            if (string.IsNullOrWhiteSpace(hfSettings.ApiKey))
            {
                throw new InvalidOperationException("HuggingFace API Key is missing from configuration.");
            }

            builder.AddOpenAIChatCompletion(
                modelId: hfSettings.ChatModelId,
                endpoint: new Uri(hfSettings.ChatEndpoint),
                apiKey: hfSettings.ApiKey);

            return builder.Build();
        });

        return services;
    }
}
