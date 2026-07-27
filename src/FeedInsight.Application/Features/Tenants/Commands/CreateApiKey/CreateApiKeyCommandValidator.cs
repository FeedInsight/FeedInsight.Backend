using FluentValidation;

namespace FeedInsight.Application.Features.Tenants.Commands.CreateApiKey;

public class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("API Key name is required.")
            .MaximumLength(100).WithMessage("API Key name cannot exceed 100 characters.");
            
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).When(x => x.ExpiresAt.HasValue).WithMessage("Expiration date must be in the future.");
    }
}
