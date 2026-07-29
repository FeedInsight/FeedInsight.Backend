using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Categories.Specifications;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.JiraSubtasks;
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
    private readonly IRepository<JiraSubtask> _jiraSubtaskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptor _encryptor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<JiraSyncService> _logger;

    public JiraSyncService(
        IRepository<Tenant> tenantRepository,
        IRepository<Category> categoryRepository,
        IRepository<UserStory> userStoryRepository,
        IRepository<JiraSubtask> jiraSubtaskRepository,
        IUnitOfWork unitOfWork,
        IEncryptor encryptor,
        IHttpClientFactory httpClientFactory,
        ILogger<JiraSyncService> logger)
    {
        _tenantRepository = tenantRepository;
        _categoryRepository = categoryRepository;
        _userStoryRepository = userStoryRepository;
        _jiraSubtaskRepository = jiraSubtaskRepository;
        _unitOfWork = unitOfWork;
        _encryptor = encryptor;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task TriggerInitialBulkSyncAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Get and decrypt jira credentials
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
            if (fallbackCategory is null) return; 

            // 3. Prepare HttpClient
            var client = _httpClientFactory.CreateClient("JiraClient");
            client.BaseAddress = new Uri(tenant.JiraBaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 4. Fetch recent active issues from Jira using JQL
            // Note: In production, you would handle pagination here using 'startAt' and 'maxResults'
            string jql = "statusCategory != Done ORDER BY created DESC";
            var response = await client.GetAsync($"/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&maxResults=100", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Jira API returned {StatusCode} for Tenant {TenantId}", response.StatusCode, tenant.Id);
                return;
            }

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(jsonString);
            var issues = document.RootElement.GetProperty("issues").EnumerateArray();

            foreach (var issue in issues)
            {
                string key = issue.GetProperty("key").GetString() ?? "";
                var fields = issue.GetProperty("fields");
                string title = fields.GetProperty("summary").GetString() ?? "Untitled";
                string issueType = fields.GetProperty("issuetype").GetProperty("name").GetString() ?? "";
                string status = fields.GetProperty("status").GetProperty("name").GetString() ?? "To Do";

                // If it's a Subtask, map to JiraSubtask
                if (issueType.Equals("Sub-task", StringComparison.OrdinalIgnoreCase))
                {
                    // To link it, we need to find its parent story in our DB.
                    // Jira stores the parent inside a specific field depending on the setup.
                    // For safety, we will just save it and a later vector search can map it properly if the parent isn't loaded yet.
                    var subtask = new JiraSubtask(
                        tenantId: tenant.Id,
                        userStoryId: Guid.Empty, // Placeholder until parent is resolved
                        jiraSubtaskKey: key,
                        title: title,
                        status: status
                    );
                    await _jiraSubtaskRepository.AddAsync(subtask, cancellationToken);
                }
                else
                {
                    // Treat as an Epic/Story/Task -> UserStory Semantic Backlog
                    var story = new UserStory(
                        tenantId: tenant.Id,
                        categoryId: fallbackCategory.Id, // Defaults to Uncategorized
                        source: UserStorySource.Jira,
                        title: title,
                        acceptanceCriteria: "", // Could be mapped from description
                        jiraTicketKey: key
                    );

                    // We must manually set the status for imported tickets
                    story.UpdateFromJiraWebhook(title, "", UserStoryStatus.Synced);
                    await _userStoryRepository.AddAsync(story, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully completed Initial Jira Sync for Tenant {TenantId}", tenant.Id);

            // Note: The Qdrant Embeddings will automatically be triggered by EF Core Domain Events
            // attached to the SaveChangesAsync method!
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during Jira sync for Tenant {TenantId}", tenantId);
        }
    }
}