namespace FeedInsight.Domain.Common.Models;

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void UndoSoftDelete()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}
