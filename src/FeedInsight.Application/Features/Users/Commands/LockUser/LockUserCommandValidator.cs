using FluentValidation;

namespace FeedInsight.Application.Features.Users.Commands.LockUser;

public class LockUserCommandValidator : AbstractValidator<LockUserCommand>
{
    public LockUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("A reason must be provided to lock the account.")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}