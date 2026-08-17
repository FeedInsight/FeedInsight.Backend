using Ardalis.Specification;
using FeedInsight.Application.Common.Interfaces;

using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.DailyAnalyticsSnapshot;
using FeedInsight.Domain.Tenants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeedInsight.Infrastructure.BackgroundJobs;

public class AnalyticsSnapshotJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AnalyticsSnapshotJob> _logger;

    public AnalyticsSnapshotJob( IServiceProvider serviceProvider,ILogger<AnalyticsSnapshotJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Analytics Snapshot Job started.");

        using var timer =
            new PeriodicTimer(TimeSpan.FromHours(24));

        do
        {
            try
            {
                _logger.LogInformation(
                    "Analytics Snapshot Job is executing...");

                using var scope =
                    _serviceProvider.CreateScope();

                var tenantRepository =
                    scope.ServiceProvider
                        .GetRequiredService<IRepository<Tenant>>();

                var analyticsService =
                    scope.ServiceProvider
                        .GetRequiredService<IAnalyticsSnapshotService>();

                var snapshotRepository =
                    scope.ServiceProvider
                        .GetRequiredService<IRepository<DailyAnalyticsSnapshot>>();

                var unitOfWork =
                    scope.ServiceProvider
                        .GetRequiredService<IUnitOfWork>();

                var tenants =
                    await tenantRepository.ListAsync(
                        stoppingToken);

                var snapshotDate =
                    DateOnly.FromDateTime(
                        DateTime.UtcNow.AddDays(-1));

                foreach (var tenant in tenants)
                {
                    var existingSnapshot =
                        await snapshotRepository.SingleOrDefaultAsync(
                            new AnalyticsSnapshotByTenantAndDateSpec(
                                tenant.Id,
                                snapshotDate),
                            stoppingToken);

                    if (existingSnapshot is not null)
                    {
                        _logger.LogInformation(
                            "Analytics snapshot already exists for Tenant {TenantId} on {SnapshotDate}.",
                            tenant.Id,
                            snapshotDate);

                        continue;
                    }

                    var analytics =
                        await analyticsService.GenerateSnapshotAsync(
                            tenant.Id,
                            snapshotDate,
                            stoppingToken);

                    var snapshot = new DailyAnalyticsSnapshot(
                        tenant.Id,
                        analytics.SnapshotDate,
                        analytics.TotalFeedbacksReceived,
                        analytics.PositiveSentimentCount,
                        analytics.NeutralSentimentCount,
                        analytics.NegativeSentimentCount,
                        analytics.TotalTasksExtracted,
                        analytics.DraftTicketsGenerated,
                        analytics.PoApprovalRatePercent,
                        analytics.TopRequestedFeaturesJson);

                    await snapshotRepository.AddAsync(
                        snapshot,
                        stoppingToken);
                }

                await unitOfWork.SaveChangesAsync(
                    stoppingToken);

                _logger.LogInformation(
                    "Analytics Snapshot Job completed successfully for {SnapshotDate}.",
                    snapshotDate);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while executing the Analytics Snapshot Job.");
            }

        } while (await timer.WaitForNextTickAsync(
            stoppingToken));

        _logger.LogInformation(
            "Analytics Snapshot Job is stopping.");
    }

    public sealed class AnalyticsSnapshotByTenantAndDateSpec
    : SingleResultSpecification<DailyAnalyticsSnapshot>
    {
        public AnalyticsSnapshotByTenantAndDateSpec(
            Guid tenantId,
            DateOnly snapshotDate)
        {
            Query
                .Where(x =>
                    x.TenantId == tenantId &&
                    x.SnapshotDate == snapshotDate &&
                    !x.IsDeleted);
        }
    }
}