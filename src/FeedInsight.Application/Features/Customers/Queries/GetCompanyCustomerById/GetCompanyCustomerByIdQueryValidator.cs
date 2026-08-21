using FluentValidation;

namespace FeedInsight.Application.Features.Customers.Queries.GetCompanyCustomerById;

public  class GetCompanyCustomerByIdQueryValidator : AbstractValidator<GetCompanyCustomerByIdQuery>
{
    public GetCompanyCustomerByIdQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();
    }
}