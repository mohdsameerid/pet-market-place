namespace PetMarketplace.Application.DTOs.Auth;

public class RegisterRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string Role { get; set; } = "Buyer"; // Buyer or Seller only
}
