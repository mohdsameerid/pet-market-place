namespace PetMarketplace.Application.DTOs.Reviews;

public class ReviewResponseDto
{
    public Guid Id { get; set; }
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string? ReviewerImageUrl { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
