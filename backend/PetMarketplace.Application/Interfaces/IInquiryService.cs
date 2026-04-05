using PetMarketplace.Application.DTOs.Inquiries;

namespace PetMarketplace.Application.Interfaces;

public interface IInquiryService
{
    Task<InquiryResponseDto> CreateInquiryAsync(Guid buyerId, Guid listingId, CreateInquiryRequestDto request, CancellationToken cancellationToken = default);
    Task<InquiryMessageResponseDto> SendMessageAsync(Guid senderId, Guid inquiryId, SendMessageRequestDto request, CancellationToken cancellationToken = default);
    Task<InquiryResponseDto> GetInquiryByIdAsync(Guid inquiryId, Guid requesterId, CancellationToken cancellationToken = default);
    Task<List<InquiryResponseDto>> GetInquiriesForListingAsync(Guid listingId, Guid sellerId, CancellationToken cancellationToken = default);
    Task<List<InquiryResponseDto>> GetInquiriesForBuyerAsync(Guid buyerId, CancellationToken cancellationToken = default);
}
