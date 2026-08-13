using System;
using System.Threading;
using System.Threading.Tasks;
using FeedInsight.Application.Features.UserStories.Commands.EditFeedInsightUserStory;
using FeedInsight.Application.Features.UserStories.Commands.SyncUserStoryToJira;
using FeedInsight.Application.Features.UserStories.Queries.GetUserStories;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Users;
using FeedInsight.Domain.UserStories.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/user-stories")]
[Authorize(Roles = Role.ProductOwner)]
public class UserStoriesController : ApiController
{
    private readonly IMediator _mediator;

    public UserStoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserStories(
        [FromQuery] UserStorySource? source,
        [FromQuery] bool? isSynced,
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserStoriesQuery(source, isSynced, searchTerm, categoryId, pageNumber, pageSize);
        var result = await _mediator.SendAsync(query, cancellationToken);

        return result.Match(
            paginatedResult => OkPaginated(
                data: paginatedResult.Items,
                currentPage: pageNumber,
                pageSize: pageSize,
                totalItems: paginatedResult.TotalCount
            ),
            errors => Problem(errors)
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditUserStory(
        [FromRoute] Guid id,
        [FromBody] EditUserStoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new EditFeedInsightUserStoryCommand(id, request.Title, request.AcceptanceCriteria);
        var result = await _mediator.SendAsync(command, cancellationToken);

        return result.Match(
            success => OkResponse(new { Message = "User story updated successfully." }),
            errors => Problem(errors)
        );
    }

    [HttpPost("{id}/sync-to-jira")]
    public async Task<IActionResult> SyncUserStoryToJira(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new SyncUserStoryToJiraCommand(id);
        var result = await _mediator.SendAsync(command, cancellationToken);

        return result.Match(
            success => OkResponse(new { Message = "User story synced to Jira successfully." }),
            errors => Problem(errors)
        );
    }
}

public class EditUserStoryRequest
{
    public string Title { get; set; } = string.Empty;
    public string? AcceptanceCriteria { get; set; }
}
