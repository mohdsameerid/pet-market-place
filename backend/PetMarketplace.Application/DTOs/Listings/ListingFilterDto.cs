namespace PetMarketplace.Application.DTOs.Listings;

public class ListingFilterDto
{
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? City { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public string? Gender { get; set; }
    public bool? IsVaccinated { get; set; }
    public bool? IsNeutered { get; set; }
    public bool? IsVetChecked { get; set; }
    public string SortBy { get; set; } = "Newest"; // Newest, PriceLow, PriceHigh, MostViewed
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
