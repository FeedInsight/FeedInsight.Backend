using FeedInsight.Domain.Common.Models;

namespace FeedInsight.Domain.Categories;

public class Category : Entity
{
    public Guid TenantId { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsSystemDefault { get; private set; }

   
    private Category() { } // constructor for ef-core

    public Category(Guid tenantId, string name, string? description, bool isSystemDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.");

        TenantId = tenantId;
        Name = name;
        Description = description;
        IsSystemDefault = isSystemDefault;
    }

    public void UpdateDetails(string name, string? description)
    {
        if (IsSystemDefault)
            throw new InvalidOperationException("System default categories cannot be modified.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.");

        Name = name;
        Description = description;
    }

    public new void SoftDelete()
    {
        if (IsSystemDefault)
            throw new InvalidOperationException("System default categories cannot be deleted.");

        base.SoftDelete();
    }
}
