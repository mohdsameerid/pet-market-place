using System.ComponentModel.DataAnnotations;

namespace PetMarketplace.Application.DTOs.Inquiries;

public class CreateInquiryRequestDto
{
    [Required, MaxLength(1000)]
    public string InitialMessage { get; set; } = string.Empty;
}
