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

        public static Error NotFound => Error.NotFound(
            code: "Users.NotFound",
            description: "The requested user could not be found.");

        public static Error IncorrectPassword => Error.Validation(
            code: "Users.IncorrectPassword",
            description: "The current password provided is incorrect.");

        public static Error NotAssociatedWithTenant => Error.Forbidden(
            code: "Users.NotAssociatedWithTenant",
            description: "You are not associated with any company/tenant.");

        public static Error CannotLockSuperAdmin => Error.Forbidden(
            code: "Users.CannotLockSuperAdmin",
            description: "You cannot lock a Super Admin account.");

        public static Error NotCustomer => Error.Validation(
        code: "Users.NotCustomer",
        description: "The selected user is not a customer.");
    }
}
