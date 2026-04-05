using PetMarketplace.Application.DTOs.Reviews;

namespace PetMarketplace.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewResponseDto> CreateAsync(Guid reviewerId, Guid sellerId, CreateReviewRequestDto request, CancellationToken cancellationToken = default);
    Task<SellerReviewSummaryDto> GetSellerReviewsAsync(Guid sellerId, CancellationToken cancellationToken = default);
}
