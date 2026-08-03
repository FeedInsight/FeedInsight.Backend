using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Users.Commands.RegisterProductOwner;

public record RegisterProductOwnerCommand(
    string CompanyName, 
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest<ErrorOr<Guid>>;
