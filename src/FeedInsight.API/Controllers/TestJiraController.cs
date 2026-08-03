using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Domain.UserStories;
using FeedInsight.Domain.UserStories.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestJiraController : ControllerBase
{
    private readonly IJiraSyncService _jiraSyncService;

    public TestJiraController(IJiraSyncService jiraSyncService)
    {
        _jiraSyncService = jiraSyncService;
    }

    [HttpPost("{tenantId}/create")]
    public async Task<IActionResult> CreateStory(Guid tenantId, [FromBody] TestJiraRequestDto request, CancellationToken cancellationToken)
    {
        // Mock a UserStory object with no JiraTicketKey to simulate creation
        var story = new UserStory(
            tenantId: tenantId,
            categoryId: Guid.NewGuid(),
            source: UserStorySource.FeedInsight,
            title: request.Title,
            acceptanceCriteria: request.AcceptanceCriteria
        );

        await _jiraSyncService.PushStoryToJiraAsync(tenantId, story, cancellationToken);
        return Ok("Creation requested. Check server logs for success or failure response from Jira.");
    }

    [HttpPut("{tenantId}/update/{issueKey}")]
    public async Task<IActionResult> UpdateStory(Guid tenantId, string issueKey, [FromBody] TestJiraRequestDto request, CancellationToken cancellationToken)
    {
        // Mock a UserStory object with a JiraTicketKey to simulate updating an existing issue
        var story = new UserStory(
            tenantId: tenantId,
            categoryId: Guid.NewGuid(),
            source: UserStorySource.FeedInsight,
            title: request.Title,
            acceptanceCriteria: request.AcceptanceCriteria,
            jiraTicketKey: issueKey
        );

        await _jiraSyncService.PushStoryToJiraAsync(tenantId, story, cancellationToken);
        return Ok($"Update requested for {issueKey}. Check server logs for success or failure response from Jira.");
    }
}

public class TestJiraRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string AcceptanceCriteria { get; set; } = string.Empty;
}
