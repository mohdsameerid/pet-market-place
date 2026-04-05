using Microsoft.EntityFrameworkCore;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Admin;
using PetMarketplace.Application.Interfaces;
using PetMarketplace.Core.Enums;
using PetMarketplace.Infrastructure.Persistence;

namespace PetMarketplace.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public AdminService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users.ToListAsync(cancellationToken);
        var listings = await _context.Listings.ToListAsync(cancellationToken);

        return new DashboardStatsDto
        {
            TotalUsers = users.Count,
            TotalBuyers = users.Count(u => u.Role == UserRole.Buyer),
            TotalSellers = users.Count(u => u.Role == UserRole.Seller),
            VerifiedSellers = users.Count(u => u.Role == UserRole.Seller && u.IsVerified),
            BannedUsers = users.Count(u => u.IsBanned),
            TotalListings = listings.Count,
            DraftListings = listings.Count(l => l.Status == ListingStatus.Draft),
            PendingListings = listings.Count(l => l.Status == ListingStatus.PendingApproval),
            ActiveListings = listings.Count(l => l.Status == ListingStatus.Active),
            RejectedListings = listings.Count(l => l.Status == ListingStatus.Rejected),
            SoldListings = listings.Count(l => l.Status == ListingStatus.Sold),
            TotalInquiries = await _context.Inquiries.CountAsync(cancellationToken),
            TotalFavorites = await _context.Favorites.CountAsync(cancellationToken)
        };
    }

    public async Task<PagedResult<AdminListingResponseDto>> GetPendingListingsAsync(
        int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Images)
            .Where(l => l.Status == ListingStatus.PendingApproval)
            .OrderBy(l => l.CreatedAt);

        return await PaginateListingsAsync(query, pageNumber, pageSize, cancellationToken);
    }

    public async Task<PagedResult<AdminListingResponseDto>> GetAllListingsAsync(
        int pageNumber, int pageSize, string? status,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<ListingStatus>(status, true, out var listingStatus))
            query = query.Where(l => l.Status == listingStatus);

        query = query.OrderByDescending(l => l.CreatedAt);

        return await PaginateListingsAsync(query, pageNumber, pageSize, cancellationToken);
    }

    public async Task<AdminListingResponseDto> ApproveListingAsync(
        Guid listingId,
        CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
            throw new KeyNotFoundException("Listing not found.");

        if (listing.Status != ListingStatus.PendingApproval)
            throw new InvalidOperationException("Only pending listings can be approved.");

        listing.Status = ListingStatus.Active;
        listing.RejectionReason = null;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAsync(
            listing.SellerId,
            "Listing Approved!",
            $"Your listing '{listing.Title}' has been approved and is now live.",
            cancellationToken);

        return MapToAdminListingDto(listing);
    }

    public async Task<AdminListingResponseDto> RejectListingAsync(
        Guid listingId, RejectListingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
            throw new KeyNotFoundException("Listing not found.");

        if (listing.Status != ListingStatus.PendingApproval)
            throw new InvalidOperationException("Only pending listings can be rejected.");

        listing.Status = ListingStatus.Rejected;
        listing.RejectionReason = request.Reason;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAsync(
            listing.SellerId,
            "Listing Rejected",
            $"Your listing '{listing.Title}' was rejected. Reason: {request.Reason}",
            cancellationToken);

        return MapToAdminListingDto(listing);
    }

    public async Task<PagedResult<AdminUserResponseDto>> GetAllUsersAsync(
        int pageNumber, int pageSize, string? role,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(role) &&
            Enum.TryParse<UserRole>(role, true, out var userRole))
            query = query.Where(u => u.Role == userRole);

        query = query.OrderByDescending(u => u.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = users.Select(u => u.Id).ToList();

        var listingCounts = await _context.Listings
            .Where(l => userIds.Contains(l.SellerId))
            .GroupBy(l => l.SellerId)
            .Select(g => new { SellerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SellerId, x => x.Count, cancellationToken);

        return new PagedResult<AdminUserResponseDto>
        {
            Items = users.Select(u => new AdminUserResponseDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                City = u.City,
                Role = u.Role.ToString(),
                IsVerified = u.IsVerified,
                IsBanned = u.IsBanned,
                TotalListings = listingCounts.GetValueOrDefault(u.Id, 0),
                CreatedAt = u.CreatedAt
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminUserResponseDto> VerifySellerAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (user.Role != UserRole.Seller)
            throw new InvalidOperationException("Only sellers can be verified.");

        user.IsVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetAdminUserDtoAsync(user.Id, cancellationToken);
    }

    public async Task<AdminUserResponseDto> BanUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("Cannot ban an admin.");

        user.IsBanned = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetAdminUserDtoAsync(user.Id, cancellationToken);
    }

    public async Task<AdminUserResponseDto> UnbanUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.IsBanned = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetAdminUserDtoAsync(user.Id, cancellationToken);
    }

    // ── Private Helpers ──────────────────────────────────────────

    private async Task<PagedResult<AdminListingResponseDto>> PaginateListingsAsync(
        IQueryable<Core.Entities.Listing> query,
        int pageNumber, int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminListingResponseDto>
        {
            Items = items.Select(MapToAdminListingDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private async Task<AdminUserResponseDto> GetAdminUserDtoAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        var listingCount = await _context.Listings
            .CountAsync(l => l.SellerId == userId, cancellationToken);

        return new AdminUserResponseDto
        {
            Id = user!.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            Role = user.Role.ToString(),
            IsVerified = user.IsVerified,
            IsBanned = user.IsBanned,
            TotalListings = listingCount,
            CreatedAt = user.CreatedAt
        };
    }

    private static AdminListingResponseDto MapToAdminListingDto(Core.Entities.Listing listing) => new()
    {
        Id = listing.Id,
        Title = listing.Title,
        Species = listing.Species.ToString(),
        Breed = listing.Breed,
        Price = listing.Price,
        City = listing.City,
        Status = listing.Status.ToString(),
        RejectionReason = listing.RejectionReason,
        IsVaccinated = listing.IsVaccinated,
        IsVetChecked = listing.IsVetChecked,
        SellerId = listing.SellerId,
        SellerName = listing.Seller?.FullName ?? string.Empty,
        SellerEmail = listing.Seller?.Email ?? string.Empty,
        IsSellerVerified = listing.Seller?.IsVerified ?? false,
        ImageCount = listing.Images?.Count ?? 0,
        CreatedAt = listing.CreatedAt
    };
}
