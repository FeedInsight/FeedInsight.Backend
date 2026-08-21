using FluentValidation;

namespace FeedInsight.Application.Features.Customers.Queries.GetCompanyCustomers;

public  class GetCompanyCustomersQueryValidator: AbstractValidator<GetCompanyCustomersQuery>
{
    public GetCompanyCustomersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}