using FeedInsight.Domain.Common.Models;

namespace FeedInsight.Domain.Users;

public class Role : Entity
{
    public string Name { get; private set; }

    public const string SuperAdmin = "SuperAdmin";
    public const string ProductOwner = "ProductOwner";
    public const string CompanyCustomer = "CompanyCustomer";


    private Role() { } // private constructor for ef-core

    public Role(string name)
    {
        Name = name;
    }
}
