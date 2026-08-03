using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Common.Models;

namespace FeedInsight.Domain.Tenants;

public class ApiKey : Entity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string KeyHash { get; private set; }

    // the first 4-12 character so the user can identify it in the UI
    public string Prefix { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsActive => DateTime.UtcNow < ExpiresAt && !IsDeleted;

    private ApiKey() { } // constructore for ef-core

    public ApiKey(Guid tenantId, string name, string plainTextKey, DateTime expiresAt, IApiKeyHasher hasher)
    {
        TenantId = tenantId;
        Name = name;

        Prefix = plainTextKey.Length > 12 ? plainTextKey.Substring(0, 12) : plainTextKey.Substring(0, 4);

        KeyHash = hasher.Hash(plainTextKey);

        ExpiresAt = expiresAt;
    }

    public void Revoke()
    {
        SoftDelete();
    }
}
