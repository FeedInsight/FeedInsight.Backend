using ErrorOr;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Categories.Specifications;
using FeedInsight.Application.Features.Jira.Specifications;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Errors;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Tenants;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Enums;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FeedInsight.Application.Features.Jira.Commands.ProcessJiraWebhook;

public class ProcessJiraWebhookCommandHandler : IRequestHandler<ProcessJiraWebhookCommand, ErrorOr<Success>>
{
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IRepository<UserStory> _userStoryRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IEncryptor _encryptor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessJiraWebhookCommandHandler> _logger;

    public ProcessJiraWebhookCommandHandler(
        IRepository<Tenant> tenantRepository,
        IRepository<UserStory> userStoryRepository,
        IRepository<Category> categoryRepository,
        IEncryptor encryptor,
        IUnitOfWork unitOfWork,
        ILogger<ProcessJiraWebhookCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _userStoryRepository = userStoryRepository;
        _categoryRepository = categoryRepository;
        _encryptor = encryptor;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> HandleAsync(ProcessJiraWebhookCommand request, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null || string.IsNullOrWhiteSpace(tenant.JiraWebhookSecret))
        {
            _logger.LogWarning("Webhook failed: Tenant {TenantId} missing or lacks Webhook Secret.", request.TenantId);
            return Errors.Tenants.NotFound;
        }

        string secret = _encryptor.Decrypt(tenant.JiraWebhookSecret);

        if (!IsSignatureValid(request.RawPayload, secret, request.SignatureHeader))
        {
            _logger.LogWarning("Webhook failed: Invalid HMAC Signature for Tenant {TenantId}.", request.TenantId);
            return Error.Unauthorized("Jira.InvalidSignature", "The webhook signature is invalid.");
        }

        try
        {
            using var document = JsonDocument.Parse(request.RawPayload);
            var root = document.RootElement;

            string webhookEvent = root.GetProperty("webhookEvent").GetString() ?? "";

            // We now care about Created, Updated, and Deleted
            var allowedEvents = new[] { "jira:issue_created", "jira:issue_updated", "jira:issue_deleted" };
            if (!allowedEvents.Contains(webhookEvent)) return Result.Success;

            var issue = root.GetProperty("issue");
            string ticketKey = issue.GetProperty("key").GetString() ?? "";

            var fields = issue.GetProperty("fields");
            string title = fields.GetProperty("summary").GetString() ?? "Untitled";
            string status = fields.GetProperty("status").GetProperty("name").GetString() ?? "To Do";
            string issueType = fields.GetProperty("issuetype").GetProperty("name").GetString() ?? "";

            bool isSubtask = issueType.Equals("Sub-task", StringComparison.OrdinalIgnoreCase);

            if (isSubtask)
            {
                return Result.Success;
            }
            var story = await _userStoryRepository.SingleOrDefaultAsync(
                    new UserStoryByJiraKeySpec(tenant.Id, ticketKey), cancellationToken);

            var mappedStatus = status.Equals("Done", StringComparison.OrdinalIgnoreCase)
                    ? UserStoryStatus.Closed
                    : UserStoryStatus.Synced;

            if (webhookEvent == "jira:issue_deleted" && story != null)
            {
                story.SoftDelete();
            }
            else if (webhookEvent == "jira:issue_updated" && story != null)
            {
                story.UpdateFromJiraWebhook(title, story.AcceptanceCriteria, mappedStatus);
            }
            else if (webhookEvent == "jira:issue_created" && story == null)
            {
                // Fetch the fallback category for new Jira stories
                var categories = await _categoryRepository.ListAsync(new CategoriesByTenantSpec(tenant.Id), cancellationToken);
                var fallbackCategory = categories.FirstOrDefault(c => c.IsSystemDefault);

                if (fallbackCategory != null)
                {
                    story = new UserStory(tenant.Id, fallbackCategory.Id, UserStorySource.Jira, title, "", ticketKey);
                    story.UpdateFromJiraWebhook(title, "", mappedStatus);
                    await _userStoryRepository.AddAsync(story, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse or process Jira Webhook Payload.");
            return Error.Failure("Jira.ParseError", "Failed to parse webhook JSON.");
        }

        return Result.Success;
    }

    private static bool IsSignatureValid(string payload, string secret, string signatureHeader)
    {
        if (!signatureHeader.StartsWith("sha256=")) return false;

        string providedHash = signatureHeader.Substring(7);

        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(secretBytes);
        var computedHashBytes = hmac.ComputeHash(payloadBytes);
        var computedHashString = Convert.ToHexString(computedHashBytes).ToLowerInvariant();

        return providedHash == computedHashString;
    }
}