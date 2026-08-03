using FluentValidation;
using FeedInsight.Domain.Tenants.Enums;

namespace FeedInsight.Application.Features.Tenants.Commands.ToggleTenantStatus;

public class ToggleTenantStatusCommandValidator
    : AbstractValidator<ToggleTenantStatusCommand>
{
    public ToggleTenantStatusCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .When(x => x.Status is TenantStatus.Suspended
                or TenantStatus.Deactivated);
    }
}