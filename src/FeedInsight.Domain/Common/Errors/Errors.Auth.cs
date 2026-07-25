using ErrorOr;

namespace FeedInsight.Domain.Common.Errors;

public static partial class Errors
{
    public static class Auth
    {
        public static Error InvalidCredentials => Error.Unauthorized(
            code: "Auth.InvalidCredentials",
            description: "The email or password provided is incorrect.");

        public static Error AccountLocked => Error.Forbidden(
            code: "Auth.AccountLocked",
            description: "This account has been locked. Please contact support.");
    }
}