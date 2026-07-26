using ErrorOr;

namespace FeedInsight.Domain.Common.Errors;

public static partial class Errors
{
    public static class Tenants
    {
        public static Error NotFound => Error.NotFound(
            code: "Tenants.NotFound",
            description: "The requested company/tenant could not be found.");
    }
}