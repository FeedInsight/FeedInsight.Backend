using FluentValidation;

namespace FeedInsight.Application.Features.Tenants.Commands.ConfigureJira
{
    public class ConfigureJiraCommandValidator : AbstractValidator<ConfigureJiraCommand>
    {
        public ConfigureJiraCommandValidator()
        {
            RuleFor(x => x.JiraUrl)
                .NotEmpty().WithMessage("Jira URL is required")
                .MaximumLength(500).WithMessage("Jira URL cannot exceed 500 characters.")
                .Matches(@"^https?:\/\/.*$").WithMessage("Jira URL must be a valid URL.");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MaximumLength(100).WithMessage("Username cannot exceed 100 characters.");

            RuleFor(x => x.ApiKey)
                .NotEmpty().WithMessage("API Key is required.")
                .MaximumLength(2000).WithMessage("API Key cannot exceed 2000 characters.");
        }
    }
}