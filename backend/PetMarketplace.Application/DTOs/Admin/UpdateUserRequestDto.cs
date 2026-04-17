namespace PetMarketplace.Application.DTOs.Admin;

public class UpdateUserRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string Role { get; set; } = string.Empty;
}
