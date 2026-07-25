using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest<ErrorOr<Guid>>;
