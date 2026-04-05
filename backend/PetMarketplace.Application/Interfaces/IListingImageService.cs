using PetMarketplace.Application.DTOs.Listings;

namespace PetMarketplace.Application.Interfaces;

public interface IListingImageService
{
    Task<UploadImageResponseDto> UploadImageAsync(Guid listingId, Guid sellerId, Stream imageStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(Guid listingId, Guid imageId, Guid sellerId, CancellationToken cancellationToken = default);
    Task SetMainImageAsync(Guid listingId, Guid imageId, Guid sellerId, CancellationToken cancellationToken = default);
}
