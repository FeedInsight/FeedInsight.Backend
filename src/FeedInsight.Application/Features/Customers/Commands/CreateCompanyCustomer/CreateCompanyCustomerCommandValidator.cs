using FluentValidation;

namespace FeedInsight.Application.Features.Customers.Commands.CreateCompanyCustomer;

public  class CreateCompanyCustomerCommandValidator: AbstractValidator<CreateCompanyCustomerCommand>
{
    public CreateCompanyCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}