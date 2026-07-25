using ErrorOr;

namespace FeedInsight.Domain.Common.Errors;

public static partial class Errors
{
    public static class Users
    {
        public static Error DuplicateEmail => Error.Conflict(
            code: "Users.DuplicateEmail",
            description: "A user with this email address already exists.");

        public static Error RoleNotFound => Error.NotFound(
            code: "Users.RoleNotFound",
            description: "The requested system role could not be found.");
    }
}
