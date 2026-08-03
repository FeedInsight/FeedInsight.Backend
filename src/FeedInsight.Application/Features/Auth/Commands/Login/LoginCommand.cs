using ErrorOr;
using FeedInsight.Application.Features.Auth.Common;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<ErrorOr<AuthenticationResult>>;
