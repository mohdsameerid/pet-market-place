namespace PetMarketplace.Core.Entities;

public class Inquiry : BaseEntity
{
    // FK
    public Guid ListingId { get; set; }
    public Guid BuyerId { get; set; }

    // Navigation
    public Listing Listing { get; set; } = null!;
    public User Buyer { get; set; } = null!;
    public ICollection<InquiryMessage> Messages { get; set; } = new List<InquiryMessage>();
}
