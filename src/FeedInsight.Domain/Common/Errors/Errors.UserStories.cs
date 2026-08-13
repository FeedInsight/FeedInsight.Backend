using ErrorOr;

namespace FeedInsight.Domain.Common.Errors;

public static partial class Errors
{
    public static class UserStories
    {
        public static Error NotFound => Error.NotFound(
            code: "UserStories.NotFound",
            description: "The requested user story could not be found.");

        public static Error InvalidSource => Error.Validation(
            code: "UserStories.InvalidSource",
            description: "Only user stories created by FeedInsight can be edited or manually synced.");

        public static Error AlreadySynced => Error.Validation(
            code: "UserStories.AlreadySynced",
            description: "This user story is already synced to Jira.");

        public static Error JiraSyncFailed => Error.Failure(
            code: "UserStories.JiraSyncFailed",
            description: "Failed to sync the user story to Jira. Please check Jira configuration and logs.");
    }
}
