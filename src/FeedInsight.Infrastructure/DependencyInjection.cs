using FeedInsight.Application.Messaging;
using FeedInsight.Infrastructure.Persistence.Context;
using FeedInsight.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Infrastructure.Persistence;
using FeedInsight.Application.Features.Feeds;
using FeedInsight.Infrastructure.Persistence.Repositories;

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

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFeedRepository, FeedRepository>();

        return services;
    }
}
