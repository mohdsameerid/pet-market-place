namespace PetMarketplace.Core.Entities;

public class InquiryMessage : BaseEntity
{
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // FK
    public Guid InquiryId { get; set; }
    public Guid SenderId { get; set; }

    // Navigation
    public Inquiry Inquiry { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
