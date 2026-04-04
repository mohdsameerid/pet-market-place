using PetMarketplace.Core.Enums;

namespace PetMarketplace.Core.Entities;

public class Listing : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsNegotiable { get; set; } = false;
    public Species Species { get; set; }
    public string? Breed { get; set; }
    public int AgeMonths { get; set; }
    public Gender Gender { get; set; }
    public string City { get; set; } = string.Empty;
    public ListingStatus Status { get; set; } = ListingStatus.Draft;
    public string? RejectionReason { get; set; }
    public bool IsVaccinated { get; set; } = false;
    public bool IsNeutered { get; set; } = false;
    public bool IsVetChecked { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // FK
    public Guid SellerId { get; set; }

    // Navigation
    public User Seller { get; set; } = null!;
    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();
    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
