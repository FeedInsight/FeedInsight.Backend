using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string FirstName,
    string LastName
) : IRequest<ErrorOr<Success>>;
