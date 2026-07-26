using FeedInsight.Application.Features.Users.Commands.LockUser;
using FeedInsight.Application.Features.Users.Commands.UnlockUser;
using FeedInsight.Application.Features.Users.Queries.GetProductOwners;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class UsersController : ApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public record LockUserRequest(string Reason);

    [HttpPost("{id:guid}/lock")]
    [Authorize(Roles = Role.SuperAdmin)]
    public async Task<IActionResult> LockUser(Guid id, [FromBody] LockUserRequest request)
    {
        var command = new LockUserCommand(id, request.Reason);
        var result = await _mediator.SendAsync(command);

        return result.Match(
            success => OkResponse(new { Message = "User account has been locked and sessions revoked." }),
            errors => Problem(errors)
        );
    }

    [HttpPost("{id:guid}/unlock")]
    [Authorize(Roles = Role.SuperAdmin)]
    public async Task<IActionResult> UnlockUser(Guid id)
    {
        var command = new UnlockUserCommand(id);
        var result = await _mediator.SendAsync(command);

        return result.Match(
            success => OkResponse(new { Message = "User account has been unlocked." }),
            errors => Problem(errors)
        );
    }

    [HttpGet("product-owners")]
    [Authorize(Roles = Role.SuperAdmin)]
    public async Task<IActionResult> GetProductOwners([FromQuery] GetProductOwnersQuery query)
    {
        var result = await _mediator.SendAsync(query);

        return result.Match(
            paginatedResult => OkPaginated(
                data: paginatedResult.Items,
                currentPage: query.Page,
                pageSize: query.PageSize,
                totalItems: paginatedResult.TotalCount
            ),
            errors => Problem(errors)
        );
    }
}
