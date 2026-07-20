using FeedInsight.Application;
using FeedInsight.Infrastructure;

namespace FeedInsight.API;

public static class DependencyInjection
{
    public static IServiceCollection AddAPI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        // register application and infrastructure services
        services
            .AddApplication()
            .AddInfrastructure(configuration);

        // add swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
