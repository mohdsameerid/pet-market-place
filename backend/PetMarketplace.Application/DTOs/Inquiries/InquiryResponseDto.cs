namespace PetMarketplace.Application.DTOs.Inquiries;

public class InquiryResponseDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public string? ListingMainImageUrl { get; set; }
    public decimal ListingPrice { get; set; }
    public Guid BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<InquiryMessageResponseDto> Messages { get; set; } = new();
}
