using System.ComponentModel.DataAnnotations;

namespace PetMarketplace.Application.DTOs.Listings;

public class CreateListingRequestDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 10000000)]
    public decimal Price { get; set; }

    public bool IsNegotiable { get; set; }

    [Required]
    public string Species { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Breed { get; set; }

    [Range(0, 300)]
    public int AgeMonths { get; set; }

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    public bool IsVaccinated { get; set; }
    public bool IsNeutered { get; set; }
    public bool IsVetChecked { get; set; }
}
