namespace PetMarketplace.Core.Entities;

public class Favorite : BaseEntity
{
    // FK
    public Guid UserId { get; set; }
    public Guid ListingId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Listing Listing { get; set; } = null!;
}
