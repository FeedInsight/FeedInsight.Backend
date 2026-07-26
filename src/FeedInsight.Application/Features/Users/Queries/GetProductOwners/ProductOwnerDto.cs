namespace FeedInsight.Application.Features.Users.Queries.GetProductOwners;

public record ProductOwnerDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string CompanyName,
    bool IsLocked,
    DateTime CreatedAt
);