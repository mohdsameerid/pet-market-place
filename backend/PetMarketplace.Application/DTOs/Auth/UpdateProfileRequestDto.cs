namespace PetMarketplace.Application.DTOs.Auth;

public class UpdateProfileRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
}
