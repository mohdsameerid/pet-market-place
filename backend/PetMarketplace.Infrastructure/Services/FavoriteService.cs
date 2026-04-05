using Microsoft.EntityFrameworkCore;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Favorites;
using PetMarketplace.Application.Interfaces;
using PetMarketplace.Core.Entities;
using PetMarketplace.Infrastructure.Persistence;

namespace PetMarketplace.Infrastructure.Services;

public class FavoriteService : IFavoriteService
{
    private readonly AppDbContext _context;

    public FavoriteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
            throw new KeyNotFoundException("Listing not found.");

        var exists = await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.ListingId == listingId, cancellationToken);

        if (exists)
            throw new InvalidOperationException("Listing is already in favorites.");

        var favorite = new Favorite
        {
            UserId = userId,
            ListingId = listingId
        };

        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ListingId == listingId, cancellationToken);

        if (favorite == null)
            throw new KeyNotFoundException("Favorite not found.");

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<FavoriteResponseDto>> GetUserFavoritesAsync(
        Guid userId, int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Favorites
            .Include(f => f.Listing)
                .ThenInclude(l => l.Images)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<FavoriteResponseDto>
        {
            Items = items.Select(f => new FavoriteResponseDto
            {
                Id = f.Id,
                ListingId = f.ListingId,
                ListingTitle = f.Listing.Title,
                ListingPrice = f.Listing.Price,
                ListingCity = f.Listing.City,
                ListingStatus = f.Listing.Status.ToString(),
                MainImageUrl = f.Listing.Images
                    .FirstOrDefault(i => i.IsMain)?.ImageUrl
                    ?? f.Listing.Images.FirstOrDefault()?.ImageUrl,
                CreatedAt = f.CreatedAt
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<bool> IsFavoritedAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default)
    {
        return await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.ListingId == listingId, cancellationToken);
    }
}
