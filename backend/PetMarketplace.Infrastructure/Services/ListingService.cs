using Microsoft.EntityFrameworkCore;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Listings;
using PetMarketplace.Application.Interfaces;
using PetMarketplace.Core.Entities;
using PetMarketplace.Core.Enums;
using PetMarketplace.Infrastructure.Persistence;

namespace PetMarketplace.Infrastructure.Services;

public class ListingService : IListingService
{
    private readonly AppDbContext _context;

    public ListingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ListingResponseDto>> GetAllAsync(ListingFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Images)
            .Where(l => l.Status == ListingStatus.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Species) && Enum.TryParse<Species>(filter.Species, true, out var species))
            query = query.Where(l => l.Species == species);

        if (!string.IsNullOrWhiteSpace(filter.Breed))
            query = query.Where(l => l.Breed != null && l.Breed.ToLower().Contains(filter.Breed.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(l => l.City.ToLower().Contains(filter.City.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.Gender) && Enum.TryParse<Gender>(filter.Gender, true, out var gender))
            query = query.Where(l => l.Gender == gender);

        if (filter.MinPrice.HasValue)
            query = query.Where(l => l.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(l => l.Price <= filter.MaxPrice.Value);

        if (filter.MinAge.HasValue)
            query = query.Where(l => l.AgeMonths >= filter.MinAge.Value);

        if (filter.MaxAge.HasValue)
            query = query.Where(l => l.AgeMonths <= filter.MaxAge.Value);

        if (filter.IsVaccinated.HasValue)
            query = query.Where(l => l.IsVaccinated == filter.IsVaccinated.Value);

        if (filter.IsNeutered.HasValue)
            query = query.Where(l => l.IsNeutered == filter.IsNeutered.Value);

        if (filter.IsVetChecked.HasValue)
            query = query.Where(l => l.IsVetChecked == filter.IsVetChecked.Value);

        query = filter.SortBy switch
        {
            "PriceLow"   => query.OrderBy(l => l.Price),
            "PriceHigh"  => query.OrderByDescending(l => l.Price),
            "MostViewed" => query.OrderByDescending(l => l.ViewCount),
            _            => query.OrderByDescending(l => l.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ListingResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<ListingResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (listing == null)
            throw new KeyNotFoundException("Listing not found.");

        return MapToDto(listing);
    }

    public async Task<ListingResponseDto> CreateAsync(Guid sellerId, CreateListingRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Species>(request.Species, true, out var species))
            throw new InvalidOperationException("Invalid species value.");

        if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
            throw new InvalidOperationException("Invalid gender value.");

        var listing = new Listing
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            IsNegotiable = request.IsNegotiable,
            Species = species,
            Breed = request.Breed,
            AgeMonths = request.AgeMonths,
            Gender = gender,
            City = request.City,
            IsVaccinated = request.IsVaccinated,
            IsNeutered = request.IsNeutered,
            IsVetChecked = request.IsVetChecked,
            SellerId = sellerId,
            Status = ListingStatus.Draft,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Listings.Add(listing);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(listing.Id, cancellationToken);
    }

    public async Task<ListingResponseDto> UpdateAsync(Guid listingId, Guid sellerId, UpdateListingRequestDto request, CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
            throw new KeyNotFoundException("Listing not found.");

        if (listing.SellerId != sellerId)
            throw new UnauthorizedAccessException("You do not own this listing.");

        if (listing.Status == ListingStatus.Active || listing.Status == ListingStatus.Sold)
            throw new InvalidOperationException("Cannot edit an active or sold listing.");

        listing.Title = request.Title;
        listing.Description = request.Description;
        listing.Price = request.Price;
        listing.IsNegotiable = request.IsNegotiable;
        listing.Breed = request.Breed;
        listing.AgeMonths = request.AgeMonths;
        listing.City = request.City;
        listing.IsVaccinated = request.IsVaccinated;
        listing.IsNeutered = request.IsNeutered;
        listing.IsVetChecked = request.IsVetChecked;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(listingId, cancellationToken);
    }

    public async Task DeleteAsync(Guid listingId, Guid requesterId, string requesterRole, CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
            throw new KeyNotFoundException("Listing not found.");

        if (requesterRole != "Admin" && listing.SellerId != requesterId)
            throw new UnauthorizedAccessException("You do not own this listing.");

        _context.Listings.Remove(listing);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ListingResponseDto> SubmitForApprovalAsync(Guid listingId, Guid sellerId, CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
            throw new KeyNotFoundException("Listing not found.");

        if (listing.SellerId != sellerId)
            throw new UnauthorizedAccessException("You do not own this listing.");

        if (listing.Status != ListingStatus.Draft && listing.Status != ListingStatus.Rejected)
            throw new InvalidOperationException("Only Draft or Rejected listings can be submitted for approval.");

        listing.Status = ListingStatus.PendingApproval;
        listing.RejectionReason = null;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(listingId, cancellationToken);
    }

    public async Task IncrementViewCountAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing != null)
        {
            listing.ViewCount++;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<PagedResult<ListingResponseDto>> GetMyListingsAsync(Guid sellerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Images)
            .Where(l => l.SellerId == sellerId)
            .OrderByDescending(l => l.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ListingResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static ListingResponseDto MapToDto(Listing listing) => new()
    {
        Id = listing.Id,
        Title = listing.Title,
        Description = listing.Description,
        Price = listing.Price,
        IsNegotiable = listing.IsNegotiable,
        Species = listing.Species.ToString(),
        Breed = listing.Breed,
        AgeMonths = listing.AgeMonths,
        Gender = listing.Gender.ToString(),
        City = listing.City,
        Status = listing.Status.ToString(),
        RejectionReason = listing.RejectionReason,
        IsVaccinated = listing.IsVaccinated,
        IsNeutered = listing.IsNeutered,
        IsVetChecked = listing.IsVetChecked,
        ViewCount = listing.ViewCount,
        SellerId = listing.SellerId,
        SellerName = listing.Seller?.FullName ?? string.Empty,
        IsSellerVerified = listing.Seller?.IsVerified ?? false,
        Images = listing.Images?.Select(i => new ListingImageDto
        {
            Id = i.Id,
            ImageUrl = i.ImageUrl,
            IsMain = i.IsMain
        }).ToList() ?? new(),
        CreatedAt = listing.CreatedAt,
        UpdatedAt = listing.UpdatedAt
    };
}
