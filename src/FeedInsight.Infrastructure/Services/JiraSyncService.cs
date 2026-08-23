using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Common.Options;
using FeedInsight.Application.Features.Categories.Specifications;
using FeedInsight.Application.Features.Jira.Models;
using FeedInsight.Application.Features.UserStories.Specifications;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FeedInsight.Infrastructure.Services;

public class JiraSyncService : IJiraSyncService
{
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<UserStory> _userStoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptor _encryptor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<JiraSyncService> _logger;
    private readonly JiraSettings _jiraSettings;

    public JiraSyncService(
        IRepository<Tenant> tenantRepository,
        IRepository<Category> categoryRepository,
        IRepository<UserStory> userStoryRepository,
        IUnitOfWork unitOfWork,
        IEncryptor encryptor,
        IHttpClientFactory httpClientFactory,
        ILogger<JiraSyncService> logger,
        IOptions<JiraSettings> jiraSettingsOptions)
    {
        _tenantRepository = tenantRepository;
        _categoryRepository = categoryRepository;
        _userStoryRepository = userStoryRepository;
        _unitOfWork = unitOfWork;
        _encryptor = encryptor;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jiraSettings = jiraSettingsOptions.Value;
    }

    private async Task<HttpClient?> CreateJiraClientAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null || string.IsNullOrWhiteSpace(tenant.JiraBaseUrl) || string.IsNullOrWhiteSpace(tenant.JiraEncryptedToken))
        {
            _logger.LogWarning("JiraSync: Tenant {TenantId} has missing credentials.", tenantId);
            return null;
        }

        string token = _encryptor.Decrypt(tenant.JiraEncryptedToken);
        var client = _httpClientFactory.CreateClient("JiraClient");
        client.BaseAddress = new Uri(tenant.JiraBaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    public async Task TriggerInitialBulkSyncAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await CreateJiraClientAsync(tenantId, cancellationToken);
            if (client == null) return;

            var categories = await _categoryRepository.ListAsync(new CategoriesByTenantSpec(tenantId), cancellationToken);
            var fallbackCategory = categories.FirstOrDefault(c => c.IsSystemDefault);
            if (fallbackCategory is null) return;

            // Build JQL to only include allowed issue types
            var allowedTypes = string.Join(", ", _jiraSettings.AllowedIssueTypes.Select(t => $"\"{t}\""));
            string jql = $"statusCategory != Done AND issuetype in ({allowedTypes}) ORDER BY updated DESC";

            int maxResults = 100;
            string? nextPageToken = null;
            bool hasMore = true;
            
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            while (hasMore)
            {
                var payload = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "jql", jql },
                    { "maxResults", maxResults },
                    { "fields", new[] { "summary", "issuetype", "status" } }
                };

                if (!string.IsNullOrEmpty(nextPageToken))
                {
                    payload.Add("nextPageToken", nextPageToken);
                }

                var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("/rest/api/3/search/jql", content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Jira API returned {StatusCode} for Tenant {TenantId}. Details: {Error}. Stopping sync.", response.StatusCode, tenantId, errorResponse);
                    break;
                }

                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                var searchResponse = JsonSerializer.Deserialize<JiraSearchResponseDto>(jsonString, jsonOptions);

                if (searchResponse?.Issues == null)
                {
                    break;
                }

                foreach (var issue in searchResponse.Issues)
                {
                    string key = issue.Key;
                    string title = issue.Fields?.Summary ?? "Untitled";
                    string status = issue.Fields?.Status?.Name ?? "To Do";
                    string issueType = issue.Fields?.Issuetype?.Name ?? "";

                    if (!_jiraSettings.AllowedIssueTypes.Contains(issueType, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var existingStory = await _userStoryRepository.SingleOrDefaultAsync(new UserStoryByJiraKeySpec(tenantId, key), cancellationToken);

                    if (existingStory is not null)
                    {
                        existingStory.UpdateFromJiraWebhook(title, existingStory.AcceptanceCriteria, UserStoryStatus.Synced);
                        await _userStoryRepository.UpdateAsync(existingStory, cancellationToken);
                    }
                    else
                    {
                        var story = new UserStory(
                            tenantId: tenantId,
                            categoryId: fallbackCategory.Id,
                            source: UserStorySource.Jira,
                            title: title,
                            acceptanceCriteria: "",
                            jiraTicketKey: key
                        );

                        story.UpdateFromJiraWebhook(title, "", UserStoryStatus.Synced);
                        await _userStoryRepository.AddAsync(story, cancellationToken);
                    }
                }

                nextPageToken = searchResponse.NextPageToken;
                hasMore = !string.IsNullOrEmpty(nextPageToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully completed Initial Jira Sync for Tenant {TenantId}", tenantId);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during Jira sync for Tenant {TenantId}", tenantId);
        }
    }

    public async Task<JiraIssueDto?> GetIssueByKeyAsync(Guid tenantId, string issueKey, CancellationToken cancellationToken = default)
    {
        var client = await CreateJiraClientAsync(tenantId, cancellationToken);
        if (client == null) return null;

        var response = await client.GetAsync($"/rest/api/3/issue/{issueKey}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch Jira issue {IssueKey} for Tenant {TenantId}.", issueKey, tenantId);
            return null;
        }

        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<JiraIssueDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private string GetJiraPriorityId(int urgencyScore)
    {
        if (_jiraSettings.PriorityMappings == null || !_jiraSettings.PriorityMappings.Any())
        {
            // Default fallback if no mappings are configured
            // Highest=1, High=2, Medium=3, Low=4, Lowest=5
            if (urgencyScore >= 8) return "1"; // Highest
            if (urgencyScore >= 6) return "2"; // High
            if (urgencyScore >= 4) return "3"; // Medium
            if (urgencyScore >= 2) return "4"; // Low
            return "5"; // Lowest
        }

        var mapping = _jiraSettings.PriorityMappings
            .OrderByDescending(m => m.MinUrgencyScore)
            .FirstOrDefault(m => urgencyScore >= m.MinUrgencyScore);

        if (mapping != null)
        {
            return mapping.PriorityId;
        }

        return _jiraSettings.PriorityMappings.OrderBy(m => m.MinUrgencyScore).First().PriorityId;
    }

    public async Task<string?> PushStoryToJiraAsync(Guid tenantId, UserStory story, CancellationToken cancellationToken = default)
    {
        var client = await CreateJiraClientAsync(tenantId, cancellationToken);
        if (client == null) return null;
        
        var fields = new System.Collections.Generic.Dictionary<string, object>
        {
            { "summary", story.Title },
            { "description", new 
                {
                    type = "doc",
                    version = 1,
                    content = new[] 
                    {
                        new {
                            type = "paragraph",
                            content = new[] 
                            {
                                new { text = story.AcceptanceCriteria ?? "", type = "text" }
                            }
                        }
                    }
                }
            },
            { "priority", new { id = GetJiraPriorityId(story.UrgencyScore) } },
            { "labels", new[] { $"Urgency:{story.UrgencyScore}" } }
        };

        if (!string.IsNullOrEmpty(story.JiraTicketKey))
        {
            var updatePayload = new { fields = fields };
            var content = new StringContent(JsonSerializer.Serialize(updatePayload), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"/rest/api/3/issue/{story.JiraTicketKey}", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to update Jira issue {IssueKey}: {Error}", story.JiraTicketKey, error);
            }
            return story.JiraTicketKey;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_jiraSettings.DefaultProjectKey))
            {
                _logger.LogWarning("Cannot create Jira issue because DefaultProjectKey is missing in JiraSettings.");
                return null;
            }

            fields["project"] = new { key = _jiraSettings.DefaultProjectKey };
            fields["issuetype"] = new { name = _jiraSettings.AllowedIssueTypes.FirstOrDefault() ?? "Story" };

            var createPayload = new { fields = fields };
            var content = new StringContent(JsonSerializer.Serialize(createPayload), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/rest/api/3/issue", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to create Jira issue: {Error}", error);
                return null;
            }
            else
            {
                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                using var document = JsonDocument.Parse(jsonString);
                if (document.RootElement.TryGetProperty("key", out var keyElement))
                {
                    var key = keyElement.GetString();
                    _logger.LogInformation("Successfully created Jira issue {IssueKey}", key);
                    return key;
                }
                return null;
            }
        }
    }
}