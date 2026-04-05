namespace PetMarketplace.Application.DTOs.Admin;

public class AdminListingResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public decimal Price { get; set; }
    public string City { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public bool IsVaccinated { get; set; }
    public bool IsVetChecked { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string SellerEmail { get; set; } = string.Empty;
    public bool IsSellerVerified { get; set; }
    public int ImageCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
