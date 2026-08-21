using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Infrastructure.BackgroundJobs;
using FeedInsight.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Infrastructure.Extensions
{
    public static class AnalyticsExtensions
    {
        public static IServiceCollection AddAnalytics(this IServiceCollection services)
        {
            services.AddScoped<
                IAnalyticsSnapshotService,
                AnalyticsSnapshotService>();

            services.AddHostedService<AnalyticsSnapshotJob>();

            return services;
        }
    }
}

