using System.ComponentModel.DataAnnotations;

namespace PetMarketplace.Application.DTOs.Reviews;

public class CreateReviewRequestDto
{
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}
