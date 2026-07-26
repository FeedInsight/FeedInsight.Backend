using ErrorOr;
using FeedInsight.Application.Features.Auth.Common;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<ErrorOr<AuthenticationResult>>;