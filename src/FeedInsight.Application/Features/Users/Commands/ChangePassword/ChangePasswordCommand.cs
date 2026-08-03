using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Users.Commands.ChangePassword;

public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword
) : IRequest<ErrorOr<Success>>;
