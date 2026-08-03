using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Application.Features.Categories.Specifications;
using FeedInsight.Domain.Categories;
using FeedInsight.Domain.Common.Interfaces;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestStoriesController : ControllerBase
{
    private static readonly IReadOnlyList<SeedStoryItem> DefaultStories = new[]
    {
        new SeedStoryItem
        {
            Title = "Fix payment gateway timeout on checkout",
            AcceptanceCriteria = "Payment requests must complete within 30 seconds. Failed timeouts should show a retry option.",
            JiraTicketKey = "SCRUM-101"
        },
        new SeedStoryItem
        {
            Title = "Handle failed credit card payments gracefully",
            AcceptanceCriteria = "When a card is declined, show a clear error message and allow the user to try another card.",
            JiraTicketKey = "SCRUM-102"
        },
        new SeedStoryItem
        {
            Title = "Add Apple Pay and Google Pay support",
            AcceptanceCriteria = "Users can complete checkout using Apple Pay or Google Pay on supported devices.",
            JiraTicketKey = "SCRUM-103"
        },
        new SeedStoryItem
        {
            Title = "Implement subscription billing for premium plans",
            AcceptanceCriteria = "Monthly and annual subscriptions are billed automatically with email receipts.",
            JiraTicketKey = "SCRUM-104"
        },
        new SeedStoryItem
        {
            Title = "Fix invoice PDF generation for international payments",
            AcceptanceCriteria = "Invoices display correct currency symbols and tax breakdown for EU customers.",
            JiraTicketKey = "SCRUM-105"
        }
    };

    private readonly IRepository<UserStory> _userStoryRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TestStoriesController(
        IRepository<UserStory> userStoryRepository,
        IRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _userStoryRepository = userStoryRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Seeds dummy user stories for a tenant. Uses default payment-related stories when the body is empty.
    /// Stories are persisted and indexed in Qdrant via domain events.
    /// </summary>
    [HttpPost("{tenantId:guid}")]
    public async Task<IActionResult> SeedStories(
        Guid tenantId,
        [FromBody] SeedStoriesRequest? request,
        CancellationToken cancellationToken)
    {
        var categoryId = await ResolveCategoryIdAsync(tenantId, request?.CategoryId, cancellationToken);
        if (!categoryId.HasValue)
        {
            return BadRequest("No category found for this tenant. Register a product owner or create a category first.");
        }

        var items = request?.Stories is { Count: > 0 }
            ? request.Stories
            : DefaultStories;

        var created = new List<SeedStoryResult>();

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Title))
            {
                continue;
            }

            var story = new UserStory(
                tenantId,
                categoryId.Value,
                UserStorySource.FeedInsight,
                item.Title,
                item.AcceptanceCriteria,
                item.JiraTicketKey);

            await _userStoryRepository.AddAsync(story, cancellationToken);

            created.Add(new SeedStoryResult(
                story.Id,
                story.Title,
                story.JiraTicketKey));
        }

        if (created.Count == 0)
        {
            return BadRequest("No valid stories to add. Each story must have a title.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            TenantId = tenantId,
            CategoryId = categoryId.Value,
            Count = created.Count,
            Stories = created
        });
    }

    private async Task<Guid?> ResolveCategoryIdAsync(
        Guid tenantId,
        Guid? requestedCategoryId,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.ListAsync(
            new CategoriesByTenantSpec(tenantId),
            cancellationToken);

        if (requestedCategoryId.HasValue &&
            categories.Any(c => c.Id == requestedCategoryId.Value))
        {
            return requestedCategoryId.Value;
        }

        var defaultCategory = categories.FirstOrDefault(c => c.IsSystemDefault);
        if (defaultCategory is not null)
        {
            return defaultCategory.Id;
        }

        return categories.FirstOrDefault()?.Id;
    }
}

public class SeedStoriesRequest
{
    public Guid? CategoryId { get; set; }

    public List<SeedStoryItem>? Stories { get; set; }
}

public class SeedStoryItem
{
    public string Title { get; set; } = string.Empty;

    public string? AcceptanceCriteria { get; set; }

    public string? JiraTicketKey { get; set; }
}

public record SeedStoryResult(Guid Id, string Title, string? JiraTicketKey);
