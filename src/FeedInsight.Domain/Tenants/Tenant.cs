using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Common.Models;
using FeedInsight.Domain.Tenants.Enums;
using System.Data;

namespace FeedInsight.Domain.Tenants;

public class Tenant : Entity
{
    public string CompanyName { get; private set; }

    public TenantStatus Status { get; private set; } = TenantStatus.Active;
    public string? StatusReason { get; private set; }
    public CompanyType CompanyType { get; private set; }

    // Jira Integrations (Encrypted before saving!)
    public string? JiraBaseUrl { get; private set; }
    public string? JiraEncryptedToken { get; private set; }
    public string? JiraWebhookSecret { get; private set; }

    private readonly List<ApiKey> _apiKeys = new();
    public IReadOnlyCollection<ApiKey> ApiKeys => _apiKeys.AsReadOnly();

    private Tenant() { } // constructor for ef-core

    public Tenant(string companyName,CompanyType companyType)
    {
        CompanyName = companyName;
        CompanyType = companyType;
    }

    public void UpdateDetails(string companyName)
    {
        CompanyName = companyName;
    }

    public void ConfigureJira(string baseUrl, string plainTextToken, string plainTextWebhookSecret, IEncryptor encryptor)
    {
        JiraBaseUrl = baseUrl;

        JiraEncryptedToken = encryptor.Encrypt(plainTextToken);
        JiraWebhookSecret = encryptor.Encrypt(plainTextWebhookSecret);
    }

    public void AddApiKey(ApiKey apiKey)
    {
        _apiKeys.Add(apiKey);
    }

    public void Suspend(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason must be provided when suspending a tenant.");
        }

        Status = TenantStatus.Suspended;
        StatusReason = reason;
        
        // Note: In the Application layer, calling this method should also trigger a Domain Event
        // or a manual call to revoke all Refresh Tokens and API Keys associated with this Tenant!
    }

    public void Deactivate(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason must be provided when deactivating a tenant.");
        }

        Status = TenantStatus.Deactivated;
        StatusReason = reason;

        // Note: In the Application layer, calling this method should also trigger a Domain Event
        // or a manual call to revoke all Refresh Tokens and API Keys associated with this Tenant!
    }

    public void Activate()
    {
        if (Status == TenantStatus.Active)
        {
            throw new InvalidOperationException("The tenant is already active.");
        }

        Status = TenantStatus.Active;
        StatusReason = null;
    }
}
