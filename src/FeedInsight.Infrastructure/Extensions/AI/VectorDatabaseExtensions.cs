using System;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Infrastructure.Options;
using FeedInsight.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Qdrant.Client;

namespace FeedInsight.Infrastructure.Extensions.AI;

public static class VectorDatabaseExtensions
{
    public static IServiceCollection AddVectorDatabase(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<QdrantSettings>>().Value;
            if (string.IsNullOrWhiteSpace(options.Url) || string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new InvalidOperationException("Qdrant configuration is missing.");
            }
            var uri = new Uri(options.Url);
            var port = uri.IsDefaultPort ? 6334 : uri.Port;
            return new QdrantClient(host: uri.Host, port: port, https: uri.Scheme == "https", apiKey: options.ApiKey);
        });

        services.AddSingleton<IVectorDatabaseService, QdrantVectorDatabaseService>();

        return services;
    }
}
