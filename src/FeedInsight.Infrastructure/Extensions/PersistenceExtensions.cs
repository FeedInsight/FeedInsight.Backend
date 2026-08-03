using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Infrastructure.Persistence;
using FeedInsight.Infrastructure.Persistence.Context;
using FeedInsight.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeedInsight.Infrastructure.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FeedInsightDbContext>(
            options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        return services;
    }
}
