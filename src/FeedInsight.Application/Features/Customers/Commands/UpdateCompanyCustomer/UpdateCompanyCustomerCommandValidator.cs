using FluentValidation;

namespace FeedInsight.Application.Features.Customers.Commands.UpdateCompanyCustomer;

public sealed class UpdateCompanyCustomerCommandValidator: AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCompanyCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();

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
    }
}