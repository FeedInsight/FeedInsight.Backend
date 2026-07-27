namespace FeedInsight.Application.Common.Interfaces;

public interface ITenantResolver
{
    Task<Guid?> ResolveTenantIdAsync(
        CancellationToken cancellationToken = default);
}