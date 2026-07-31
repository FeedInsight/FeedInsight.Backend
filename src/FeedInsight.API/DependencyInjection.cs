using FeedInsight.API.Extensions;
using FeedInsight.API.Services;
using FeedInsight.Application;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Infrastructure;

namespace FeedInsight.API;

public static class DependencyInjection
{
    public static IServiceCollection AddAPI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentApiKeyService, CurrentApiKeyService>();
        services.AddScoped<ITenantResolver, TenantResolver>();

        services.AddJwtAuthentication(configuration);
        services.AddFrontendCorsPolicy(configuration);

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
