using ErrorOr;

namespace FeedInsight.Domain.Common.Errors;

public static partial class Errors
{
    public static class Chat
    {
        public static Error SessionNotFound => Error.NotFound(
            code: "Chat.SessionNotFound",
            description: "The requested chat session was not found.");

        public static Error MessageNotFound => Error.NotFound(
            code: "Chat.MessageNotFound",
            description: "The requested chat message was not found.");

        public static Error EmptyMessage => Error.Validation(
            code: "Chat.EmptyMessage",
            description: "Message content cannot be empty.");

        public static Error SessionAlreadyDeleted => Error.Conflict(
            code: "Chat.SessionAlreadyDeleted",
            description: "The chat session has already been deleted.");

        public static Error AiUnavailable => Error.Failure(
            code: "Chat.AiUnavailable",
            description: "The AI assistant is currently unavailable.");

        public static Error VectorSearchFailed => Error.Failure(
            code: "Chat.VectorSearchFailed",
            description: "Failed to retrieve semantic context.");

        public static Error EmbeddingGenerationFailed => Error.Failure(
            code: "Chat.EmbeddingGenerationFailed",
            description: "Failed to generate embeddings.");
    }
}