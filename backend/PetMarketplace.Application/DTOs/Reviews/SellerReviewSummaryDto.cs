namespace PetMarketplace.Application.DTOs.Reviews;

public class SellerReviewSummaryDto
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ReviewResponseDto> Reviews { get; set; } = new();
}
