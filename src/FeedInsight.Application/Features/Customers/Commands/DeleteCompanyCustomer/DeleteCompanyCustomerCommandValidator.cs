using FluentValidation;

namespace FeedInsight.Application.Features.CompanyCustomers.Commands.DeleteCompanyCustomer;

public  class DeleteCompanyCustomerCommandValidator: AbstractValidator<DeleteCompanyCustomerCommand>
{
    public DeleteCompanyCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();
    }
}