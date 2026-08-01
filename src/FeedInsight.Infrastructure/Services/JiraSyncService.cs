using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Categories.Specifications;
using FeedInsight.Application.Features.UserStories.Specifications;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Enums;
using Microsoft.Extensions.Logging;
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

    public JiraSyncService(
        IRepository<Tenant> tenantRepository,
        IRepository<Category> categoryRepository,
        IRepository<UserStory> userStoryRepository,
        IUnitOfWork unitOfWork,
        IEncryptor encryptor,
        IHttpClientFactory httpClientFactory,
        ILogger<JiraSyncService> logger)
    {
        _tenantRepository = tenantRepository;
        _categoryRepository = categoryRepository;
        _userStoryRepository = userStoryRepository;
        _unitOfWork = unitOfWork;
        _encryptor = encryptor;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task TriggerInitialBulkSyncAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Fetch Tenant & Decrypt Credentials
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null || string.IsNullOrWhiteSpace(tenant.JiraBaseUrl) || string.IsNullOrWhiteSpace(tenant.JiraEncryptedToken))
            {
                _logger.LogWarning("JiraSync aborted. Tenant {TenantId} has missing credentials.", tenantId);
                return;
            }

            string token = _encryptor.Decrypt(tenant.JiraEncryptedToken);

            // 2. Fetch the "Uncategorized" Category to act as a safe fallback
            var categories = await _categoryRepository.ListAsync(new CategoriesByTenantSpec(tenant.Id), cancellationToken);
            var fallbackCategory = categories.FirstOrDefault(c => c.IsSystemDefault);
            if (fallbackCategory is null) return; // Should never happen due to DB seeding

            // 3. Prepare HttpClient
            var client = _httpClientFactory.CreateClient("JiraClient");
            client.BaseAddress = new Uri(tenant.JiraBaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 4. Fetch active issues from Jira using JQL with Pagination
            // Added 'issuetype in standardIssueTypes()' to explicitly tell Jira NOT to send sub-tasks.
            string jql = "statusCategory != Done AND issuetype in standardIssueTypes() ORDER BY updated DESC";

            int maxResults = 100;
            string? nextPageToken = null;
            bool hasMore = true;

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
                    _logger.LogError("Jira API returned {StatusCode} for Tenant {TenantId}. Details: {Error}. Stopping sync.", response.StatusCode, tenant.Id, errorResponse);
                    break;
                }

                var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                using var document = JsonDocument.Parse(jsonString);
                var root = document.RootElement;

                var issues = root.GetProperty("issues").EnumerateArray();

                foreach (var issue in issues)
                {
                    // Defensively parse key
                    string key = issue.TryGetProperty("key", out var keyElement) && keyElement.ValueKind == JsonValueKind.String
                        ? keyElement.GetString() ?? ""
                        : "";

                    // Defensively parse fields object
                    if (!issue.TryGetProperty("fields", out var fields) || fields.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    // Defensively parse summary
                    string title = "Untitled";
                    if (fields.TryGetProperty("summary", out var summaryElement) && summaryElement.ValueKind == JsonValueKind.String)
                    {
                        title = summaryElement.GetString() ?? "Untitled";
                    }

                    // Defensively parse issuetype
                    string issueType = "";
                    if (fields.TryGetProperty("issuetype", out var issueTypeElement) && issueTypeElement.ValueKind == JsonValueKind.Object)
                    {
                        if (issueTypeElement.TryGetProperty("name", out var typeNameElement) && typeNameElement.ValueKind == JsonValueKind.String)
                        {
                            issueType = typeNameElement.GetString() ?? "";
                        }
                    }

                    // Fallback defense in case a custom sub-task type slips past standardIssueTypes()
                    if (issueType.Contains("Sub-task", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Defensively parse status
                    string status = "To Do";
                    if (fields.TryGetProperty("status", out var statusElement) && statusElement.ValueKind == JsonValueKind.Object)
                    {
                        if (statusElement.TryGetProperty("name", out var statusNameElement) && statusNameElement.ValueKind == JsonValueKind.String)
                        {
                            status = statusNameElement.GetString() ?? "To Do";
                        }
                    }

                    var existingStory = await _userStoryRepository.SingleOrDefaultAsync(new UserStoryByJiraKeySpec(tenant.Id, key), cancellationToken);

                    if (existingStory is not null)
                    {
                        // Update existing issue instead of duplicating it
                        existingStory.UpdateFromJiraWebhook(title, existingStory.AcceptanceCriteria, UserStoryStatus.Synced);
                        await _userStoryRepository.UpdateAsync(existingStory, cancellationToken);
                    }
                    else
                    {
                        // Treat all standard issues (Epic, Story, Task, Bug) as a generic UserStory in our Domain
                        var story = new UserStory(
                            tenantId: tenant.Id,
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

                if (root.TryGetProperty("nextPageToken", out var tokenProp) && tokenProp.ValueKind == JsonValueKind.String)
                {
                    nextPageToken = tokenProp.GetString();
                    hasMore = !string.IsNullOrEmpty(nextPageToken);
                }
                else
                {
                    hasMore = false;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully completed Initial Jira Sync for Tenant {TenantId}", tenant.Id);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during Jira sync for Tenant {TenantId}", tenantId);
        }
    }
}