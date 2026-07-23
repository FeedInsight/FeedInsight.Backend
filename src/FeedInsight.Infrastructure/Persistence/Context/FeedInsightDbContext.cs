using FeedInsight.Domain.Common.Models;
using FeedInsight.Domain.Feeds;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FeedInsight.Infrastructure.Persistence.Context
{
    public class FeedInsightDbContext: DbContext
    {
        public FeedInsightDbContext(DbContextOptions<FeedInsightDbContext> options): base(options) { }

        public virtual DbSet<Feed> Feeds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // This single line automatically finds ANY class in this project that implements 
            // IEntityTypeConfiguration<T> and applies its configurations!
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FeedInsightDbContext).Assembly);

            // Automatically apply the soft-delete Global Query Filter to all Entity classes
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Check if the entity inherits from our base Entity class
                if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
                {
                    // Dynamically construct the expression: e => e.IsDeleted == false
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(Entity.IsDeleted));
                    var falseConstant = Expression.Constant(false);
                    var body = Expression.Equal(property, falseConstant);
                    var lambda = Expression.Lambda(body, parameter);

                    // Apply the filter
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<Entity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
