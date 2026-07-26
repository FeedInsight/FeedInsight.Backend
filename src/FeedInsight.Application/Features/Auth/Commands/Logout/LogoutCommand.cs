using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(
    string RefreshToken
) : IRequest<ErrorOr<Success>>; 