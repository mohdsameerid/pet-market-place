namespace PetMarketplace.Application.DTOs.Listings;

public class ListingResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsNegotiable { get; set; }
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public int AgeMonths { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public bool IsVaccinated { get; set; }
    public bool IsNeutered { get; set; }
    public bool IsVetChecked { get; set; }
    public int ViewCount { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public bool IsSellerVerified { get; set; }
    public List<ListingImageDto> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
