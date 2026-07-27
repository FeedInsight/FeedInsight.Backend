using ErrorOr;

namespace FeedInsight.Application.Common.Interfaces;

public interface ITenantStatusChecker
{
    Task<ErrorOr<Success>> EnsureActiveAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}