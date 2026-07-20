using FeedInsight.Application.Messaging;
using FeedInsight.Infrastructure.Data.Context;
using FeedInsight.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // register concrete mediator implementation
        services.AddSingleton<IMediator, Mediator>();

        services.AddDbContext<FeedInsightDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        return services;
    }
}
