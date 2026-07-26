using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Users.Commands.UnlockUser;

public record UnlockUserCommand(
    Guid UserId
) : IRequest<ErrorOr<Success>>;
