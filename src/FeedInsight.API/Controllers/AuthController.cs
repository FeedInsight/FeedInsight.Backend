using FeedInsight.Application.Features.Auth.Commands.Login;
using FeedInsight.Application.Features.Users.Commands.RegisterUser;
using FeedInsight.Application.Messaging;
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
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            authResult => OkResponse(authResult),
            errors => Problem(errors)
        );
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.SendAsync(command);

        return result.Match(
            userId => OkResponse(new { UserId = userId }),
            errors => Problem(errors)
        );
    }
}
