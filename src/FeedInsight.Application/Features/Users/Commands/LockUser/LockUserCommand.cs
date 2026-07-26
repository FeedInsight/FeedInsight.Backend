using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Users.Commands.LockUser;

public record LockUserCommand(
    Guid UserId,
    string Reason
) : IRequest<ErrorOr<Success>>;
