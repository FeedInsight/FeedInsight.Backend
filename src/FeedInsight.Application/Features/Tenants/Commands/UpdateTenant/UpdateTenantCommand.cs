using ErrorOr;
using FeedInsight.Application.Messaging;

namespace FeedInsight.Application.Features.Tenants.Commands.UpdateTenant;

public record UpdateTenantCommand(
    string CompanyName
) : IRequest<ErrorOr<Success>>;
