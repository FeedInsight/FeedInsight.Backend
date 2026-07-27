using ErrorOr;

namespace FeedInsight.Domain.Common.Errors;

public static partial class Errors
{
    public static class Auth
    {
        public static Error InvalidCredentials => Error.Unauthorized(
            code: "Auth.InvalidCredentials",
            description: "The email or password provided is incorrect.");

        public static Error SessionExpired => Error.Unauthorized(
            code: "Auth.SessionExpired",
            description: "Your session has expired or is invalid. Please log in again.");

        public static Error AccountLocked => Error.Forbidden(
            code: "Auth.AccountLocked",
            description: "This account has been locked. Please contact support.");

        public static Error Unauthenticated => Error.Unauthorized(
            code: "Auth.Unauthenticated",
            description: "User is not authenticated.");

        public static Error TenantNotActive => Error.Forbidden(
             code: "Auth.TenantNotActive",
             description: "Login is not allowed because your company account is not active.");
    }
}