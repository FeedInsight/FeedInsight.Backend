using ErrorOr;

namespace FeedInsight.Domain.Common.Errors;

public static partial class Errors
{
    public static class Categories
    {
        public static Error NotFound => Error.NotFound(
            code: "Categories.NotFound",
            description: "The requested category could not be found.");

        public static Error DuplicateName => Error.Conflict(
            code: "Categories.DuplicateName",
            description: "A category with this name already exists in your company.");

        public static Error CannotModifySystemDefault => Error.Forbidden(
            code: "Categories.CannotModifySystemDefault",
            description: "System default categories (like 'Uncategorized') cannot be modified or deleted.");
    }
}