using ErrorOr;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Tenants.Enums;

namespace FeedInsight.Application.Features.Users.Commands.RegisterProductOwner;

public record RegisterProductOwnerCommand(
    string CompanyName, 
    string FirstName,
    string LastName,
    string Email,
    string Password,
    CompanyType CompanyType
) : IRequest<ErrorOr<Guid>>;
