using Microsoft.EntityFrameworkCore;
using PetMarketplace.Application.DTOs.Listings;
using PetMarketplace.Application.Interfaces;
using PetMarketplace.Core.Entities;
using PetMarketplace.Infrastructure.Persistence;

namespace PetMarketplace.Infrastructure.Services;

public class ListingImageService : IListingImageService
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;

    public ListingImageService(AppDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<UploadImageResponseDto> UploadImageAsync(
        Guid listingId, Guid sellerId,
        Stream imageStream, string fileName,
        CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
            throw new KeyNotFoundException("Listing not found.");

        if (listing.SellerId != sellerId)
            throw new UnauthorizedAccessException("You do not own this listing.");

        if (listing.Images.Count >= 6)
            throw new InvalidOperationException("Maximum 6 images allowed per listing.");

        var (imageUrl, publicId) = await _imageService.UploadImageAsync(imageStream, fileName, cancellationToken);

        var image = new ListingImage
        {
            ListingId = listingId,
            ImageUrl = imageUrl,
            PublicId = publicId,
            IsMain = !listing.Images.Any() // first image auto-set as main
        };

        _context.ListingImages.Add(image);
        await _context.SaveChangesAsync(cancellationToken);

        return new UploadImageResponseDto
        {
            Id = image.Id,
            ImageUrl = image.ImageUrl,
            IsMain = image.IsMain
        };
    }

    public async Task DeleteImageAsync(
        Guid listingId, Guid imageId, Guid sellerId,
        CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
            throw new KeyNotFoundException("Listing not found.");

        if (listing.SellerId != sellerId)
            throw new UnauthorizedAccessException("You do not own this listing.");

        var image = await _context.ListingImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ListingId == listingId, cancellationToken);

        if (image == null)
            throw new KeyNotFoundException("Image not found.");

        await _imageService.DeleteImageAsync(image.PublicId, cancellationToken);

        _context.ListingImages.Remove(image);
        await _context.SaveChangesAsync(cancellationToken);

        // If deleted image was main, auto-assign next available image as main
        if (image.IsMain)
        {
            var next = await _context.ListingImages
                .Where(i => i.ListingId == listingId)
                .FirstOrDefaultAsync(cancellationToken);

            if (next != null)
            {
                next.IsMain = true;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task SetMainImageAsync(
        Guid listingId, Guid imageId, Guid sellerId,
        CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
            throw new KeyNotFoundException("Listing not found.");

        if (listing.SellerId != sellerId)
            throw new UnauthorizedAccessException("You do not own this listing.");

        var images = await _context.ListingImages
            .Where(i => i.ListingId == listingId)
            .ToListAsync(cancellationToken);

        if (!images.Any(i => i.Id == imageId))
            throw new KeyNotFoundException("Image not found.");

        foreach (var img in images)
            img.IsMain = img.Id == imageId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
