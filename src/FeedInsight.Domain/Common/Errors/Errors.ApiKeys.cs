using ErrorOr;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Domain.Common.Errors;

public static partial class Errors
{
    public static class ApiKeys
    {
        public static Error NotFound => Error.NotFound(
            code: "ApiKeys.NotFound",
            description: "The requested API key could not be found.");

        public static Error LimitReached => Error.Conflict(
            code: "ApiKeys.LimitReached",
            description: "You have reached the maximum number of active API keys for your plan.");

        public static Error InvalidOrExpired => Error.Unauthorized(
            code: "ApiKeys.InvalidOrExpired",
            description: "The provided API key is invalid, revoked, or expired.");
    }
}