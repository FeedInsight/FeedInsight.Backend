using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Models;
using FeedInsight.Domain.CustomerFeedbacks;
using FeedInsight.Domain.ExtractedTasks;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FeedInsight.Infrastructure.Persistence.Context
{
    public class FeedInsightDbContext: DbContext
    {
        public FeedInsightDbContext(DbContextOptions<FeedInsightDbContext> options): base(options) { }

        // Pure Domain DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
        public DbSet<CustomerFeedback> CustomerFeedbacks => Set<CustomerFeedback>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<ExtractedTask> ExtractedTasks => Set<ExtractedTask>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // This single line automatically finds ANY class in this project that implements 
            // IEntityTypeConfiguration<T> and applies its configurations!
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FeedInsightDbContext).Assembly);

            // Apply global conventions to all entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // 1. GLOBAL GUID CONFIGURATION
                // If the entity has an 'Id' property of type Guid, force EF Core to accept our Domain-generated Guids
                var idProperty = entityType.FindProperty("Id");
                if (idProperty != null && idProperty.ClrType == typeof(Guid))
                {
                    idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
                }

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