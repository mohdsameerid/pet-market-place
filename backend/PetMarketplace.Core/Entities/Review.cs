namespace PetMarketplace.Core.Entities;

public class Review : BaseEntity
{
    public int Rating { get; set; } // 1 to 5
    public string? Comment { get; set; }

    // FK
    public Guid ReviewerId { get; set; }
    public Guid SellerId { get; set; }

    // Navigation
    public User Reviewer { get; set; } = null!;
    public User Seller { get; set; } = null!;
}
