using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Listings;

namespace PetMarketplace.Application.Interfaces;

public interface IListingService
{
    Task<PagedResult<ListingResponseDto>> GetAllAsync(ListingFilterDto filter, CancellationToken cancellationToken = default);
    Task<ListingResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ListingResponseDto> CreateAsync(Guid sellerId, CreateListingRequestDto request, CancellationToken cancellationToken = default);
    Task<ListingResponseDto> UpdateAsync(Guid listingId, Guid sellerId, UpdateListingRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid listingId, Guid requesterId, string requesterRole, CancellationToken cancellationToken = default);
    Task<ListingResponseDto> SubmitForApprovalAsync(Guid listingId, Guid sellerId, CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<PagedResult<ListingResponseDto>> GetMyListingsAsync(Guid sellerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
