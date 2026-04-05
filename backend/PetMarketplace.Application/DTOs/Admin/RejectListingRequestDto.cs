using System.ComponentModel.DataAnnotations;

namespace PetMarketplace.Application.DTOs.Admin;

public class RejectListingRequestDto
{
    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
