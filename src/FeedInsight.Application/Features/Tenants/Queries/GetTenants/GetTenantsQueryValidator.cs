using FluentValidation;

namespace FeedInsight.Application.Features.Tenants.Queries.GetTenants;

public class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    public GetTenantsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .When(x => x.Page.HasValue);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(100)
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x)
            .Must(x => x.Page.HasValue == x.PageSize.HasValue)
            .WithMessage("Page and PageSize must either both be provided or both be omitted.");
    }
}