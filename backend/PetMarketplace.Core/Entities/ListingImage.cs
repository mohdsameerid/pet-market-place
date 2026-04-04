namespace PetMarketplace.Core.Entities;

public class ListingImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty; // Cloudinary public id
    public bool IsMain { get; set; } = false;

    // FK
    public Guid ListingId { get; set; }

    // Navigation
    public Listing Listing { get; set; } = null!;
}
