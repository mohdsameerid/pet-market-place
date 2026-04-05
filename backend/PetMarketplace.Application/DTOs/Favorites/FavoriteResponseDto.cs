namespace PetMarketplace.Application.DTOs.Favorites;

public class FavoriteResponseDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public decimal ListingPrice { get; set; }
    public string ListingCity { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }
    public string ListingStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
