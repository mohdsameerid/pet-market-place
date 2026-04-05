using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Favorites;

namespace PetMarketplace.Application.Interfaces;

public interface IFavoriteService
{
    Task AddAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default);
    Task<PagedResult<FavoriteResponseDto>> GetUserFavoritesAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> IsFavoritedAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default);
}
