using FeedInsight.Domain.DailyAnalyticsSnapshot;
using FeedInsight.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Infrastructure.Persistence.Configurations.DailyAnalyticsSnapshots
{
    public class DailyAnalyticsSnapshotConfiguration: IEntityTypeConfiguration<DailyAnalyticsSnapshot>
    {
        public void Configure(
            EntityTypeBuilder<DailyAnalyticsSnapshot> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SnapshotDate)
                .IsRequired();

            builder.Property(x => x.PoApprovalRatePercent)
                .HasPrecision(5, 2)
                .IsRequired();

            builder.Property(x => x.TopRequestedFeaturesJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            // Every snapshot belongs to one tenant.
            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // One snapshot per tenant per day.
            builder.HasIndex(x => new
            {
                x.TenantId,
                x.SnapshotDate
            })
            .IsUnique();
        }
    }
}
