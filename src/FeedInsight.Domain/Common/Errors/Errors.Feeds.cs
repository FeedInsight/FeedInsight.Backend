using ErrorOr;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Domain.Common.Errors;

public static partial class Errors
{
    public static class Feeds
    {
        public static Error NotFound => Error.NotFound(
            code: "Feeds.NotFound",
            description: "The requested feed could not be found.");

        public static Error TitleTaken => Error.Conflict(
            code: "Feeds.TitleTaken",
            description: "A feed with this title already exists.");

        public static Error InvalidTitle => Error.Validation(
            code: "Feeds.InvalidTitle",
            description: "The provided title contains forbidden words (e.g., 'Spam').");

        public static Error PageSizeExceeded => Error.Forbidden(
            code: "Feeds.PageSizeExceeded",
            description: "You cannot request more than 50 items per page on the free tier.");

        public static Error InvalidSearch => Error.Failure(
            code: "Feeds.InvalidSearch",
            description: "This search term violates our safety policies.");

        public static Error InvalidUrl => Error.Validation(
            code: "Feeds.InvalidUrl",
            description: "The provided feed URL is not a valid web address.");
    }
}
