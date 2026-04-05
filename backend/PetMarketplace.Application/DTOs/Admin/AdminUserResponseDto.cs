namespace PetMarketplace.Application.DTOs.Admin;

public class AdminUserResponseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public bool IsBanned { get; set; }
    public int TotalListings { get; set; }
    public DateTime CreatedAt { get; set; }
}
