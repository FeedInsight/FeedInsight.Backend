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

            RuleFor(x => x.PersonalAccessToken)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(x => x.WebHookSecret)
                .NotEmpty().WithMessage("API Key is required.");
        }
    }
}