using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Tenants.Commands.CreateTenantOwner;

public record CreateTenantOwnerCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest<ErrorOr<Guid>>;
