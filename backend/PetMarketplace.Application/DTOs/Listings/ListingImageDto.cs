namespace PetMarketplace.Application.DTOs.Listings;

public class ListingImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}
