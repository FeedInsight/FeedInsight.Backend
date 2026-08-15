using FeedInsight.Application.Features.AI.ProductAssistant;
using FeedInsight.Infrastructure.AI.ProductAssistant;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Infrastructure.Extensions.AI;

public static class ProductAssistantExtensions
{
    public static IServiceCollection AddProductAssistant(this IServiceCollection services)
    {
        services.AddScoped<IProductAssistantService, ProductAssistantService>();
        services.AddScoped<IProductAssistantContextService, ProductAssistantContextService>();
        return services;
    }
}
