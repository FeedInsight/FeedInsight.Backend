using FluentValidation;

namespace FeedInsight.Application.Features.Users.Queries.GetProductOwners;

public class GetProductOwnersQueryValidator : AbstractValidator<GetProductOwnersQuery>
{
    public GetProductOwnersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or greater.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Cannot fetch more than 100 records at a time.");
    }
}