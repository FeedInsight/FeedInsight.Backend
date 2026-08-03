using FeedInsight.API.Extensions;
using FeedInsight.API.Services;
using FeedInsight.Application;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Options;
using FeedInsight.Infrastructure;
using Qdrant.Client;

namespace FeedInsight.API;

public static class DependencyInjection
{
    public static IServiceCollection AddAPI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services.AddHttpContextAccessor();

        services.Configure<JiraSettings>(configuration.GetSection(JiraSettings.SectionName));

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentApiKeyService, CurrentApiKeyService>();
        services.AddScoped<ITenantResolver, TenantResolver>();

        services.AddJwtAuthentication(configuration);
        services.AddFrontendCorsPolicy(configuration);

        services.AddSingleton<QdrantClient>(sp =>
        {
            var url = configuration["Qdrant:Url"]
                      ?? throw new ArgumentNullException("Qdrant:Url is missing in appsettings.json");

            // Allow missing/empty API key for local Docker development
            var apiKey = configuration["Qdrant:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = null;
            }

            // Safely parse the URL string into its components
            var uri = new Uri(url);

            // If port is explicitly provided (e.g. 6334 locally), use it. 
            // Otherwise, default to standard HTTPS (443) for cloud instances.
            int port = uri.IsDefaultPort ? (uri.Scheme == "https" ? 443 : 6334) : uri.Port;
            bool isHttps = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);

            // Use named arguments to map perfectly to the SDK constructor
            return new QdrantClient(
                host: uri.Host,
                port: port,
                https: isHttps,
                apiKey: apiKey
            );
        });

        // register application and infrastructure services
        services
            .AddApplication()
            .AddInfrastructure(configuration);

        // add swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSwaggerDocumentation();

        return services;
    }
}
