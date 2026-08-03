using ErrorOr;
using FeedInsight.Application.Messaging;
using FeedInsight.Domain.Tenants.Enums;

namespace FeedInsight.Application.Features.Tenants.Commands.ToggleTenantStatus;

public record ToggleTenantStatusCommand(
    Guid TenantId,
    TenantStatus Status,
    string? Reason
) : IRequest<ErrorOr<Success>>;