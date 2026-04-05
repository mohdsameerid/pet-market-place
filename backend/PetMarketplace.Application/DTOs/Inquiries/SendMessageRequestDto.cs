using System.ComponentModel.DataAnnotations;

namespace PetMarketplace.Application.DTOs.Inquiries;

public class SendMessageRequestDto
{
    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
}
