using FeedInsight.Application.Features.Auth.Commands.Login;
using FeedInsight.Application.Features.Auth.Commands.Logout;
using FeedInsight.Application.Features.Auth.Commands.RefreshToken;
using FeedInsight.Application.Features.Users.Commands.RegisterProductOwner;
using FeedInsight.Application.Features.Users.Commands.RegisterUser;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedInsight.API.Controllers;

[Route("api/[controller]")]
public class AuthController : ApiController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            authResult => OkResponse(authResult),
            errors => Problem(errors)
        );
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            authResult => OkResponse(authResult),
            errors => Problem(errors)
        );
    }

    [HttpPost("logout")]
    [Authorize] 
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            success => OkResponse(new { Message = "Logged out successfully" }),
            errors => Problem(errors)
        );
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterProductOwner([FromBody] RegisterProductOwnerCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            userId => OkResponse(new { UserId = userId }),
            errors => Problem(errors)
        );
    }

    [HttpPost("register-admin")]
  ///  [Authorize(Roles = Role.SuperAdmin)]
    public async Task<IActionResult> RegisterSuperAdmin([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            userId => OkResponse(new { UserId = userId }),
            errors => Problem(errors)
        );
    }
}
