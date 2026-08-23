using FeedInsight.Application.Features.Users.Commands.ChangePassword;
using FeedInsight.Application.Features.Users.Commands.UpdateProfile;
using FeedInsight.Application.Features.Users.Queries.GetProfile;
using FeedInsight.Application.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ProfileController : ApiController
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            success => OkResponse(new { Message = "Profile updated successfully." }),
            errors => Problem(errors)
        );
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            success => OkResponse(new { Message = "Password changed successfully. All other sessions have been logged out." }),
            errors => Problem(errors)
        );
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _mediator.SendAsync(
            new GetProfileQuery());

        return result.Match(
            profile => OkResponse(profile),
            errors => Problem(errors));
    }
}