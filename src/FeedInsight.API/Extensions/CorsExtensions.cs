namespace FeedInsight.API.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "FrontendPolicy";

    public static IServiceCollection AddFrontendCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        // Fetch the allowed origins from appsettings.json
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, builder =>
            {
                if (allowedOrigins.Length == 0)
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                }
                else
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                }
            });
        });

        return services;
    }
}
