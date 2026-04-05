using Microsoft.EntityFrameworkCore;
using PetMarketplace.Application.DTOs.Inquiries;
using PetMarketplace.Application.Interfaces;
using PetMarketplace.Core.Entities;
using PetMarketplace.Core.Enums;
using PetMarketplace.Infrastructure.Persistence;

namespace PetMarketplace.Infrastructure.Services;

public class InquiryService : IInquiryService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public InquiryService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<InquiryResponseDto> CreateInquiryAsync(
        Guid buyerId, Guid listingId,
        CreateInquiryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
            throw new KeyNotFoundException("Listing not found.");

        if (listing.Status != ListingStatus.Active)
            throw new InvalidOperationException("You can only inquire about active listings.");

        if (listing.SellerId == buyerId)
            throw new InvalidOperationException("You cannot inquire about your own listing.");

        var existingInquiry = await _context.Inquiries
            .FirstOrDefaultAsync(i => i.ListingId == listingId && i.BuyerId == buyerId, cancellationToken);

        if (existingInquiry != null)
            throw new InvalidOperationException("You already have an open inquiry for this listing.");

        var inquiry = new Inquiry
        {
            ListingId = listingId,
            BuyerId = buyerId
        };

        _context.Inquiries.Add(inquiry);
        await _context.SaveChangesAsync(cancellationToken);

        var message = new InquiryMessage
        {
            InquiryId = inquiry.Id,
            SenderId = buyerId,
            Message = request.InitialMessage,
            SentAt = DateTime.UtcNow
        };

        _context.InquiryMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAsync(
            listing.SellerId,
            "New Inquiry",
            $"You have a new inquiry on your listing '{listing.Title}'.",
            cancellationToken);

        return await GetInquiryByIdAsync(inquiry.Id, buyerId, cancellationToken);
    }

    public async Task<InquiryMessageResponseDto> SendMessageAsync(
        Guid senderId, Guid inquiryId,
        SendMessageRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var inquiry = await _context.Inquiries
            .Include(i => i.Listing)
            .FirstOrDefaultAsync(i => i.Id == inquiryId, cancellationToken);

        if (inquiry == null)
            throw new KeyNotFoundException("Inquiry not found.");

        var isBuyer = inquiry.BuyerId == senderId;
        var isSeller = inquiry.Listing.SellerId == senderId;

        if (!isBuyer && !isSeller)
            throw new UnauthorizedAccessException("You are not part of this inquiry.");

        var sender = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == senderId, cancellationToken);

        var message = new InquiryMessage
        {
            InquiryId = inquiryId,
            SenderId = senderId,
            Message = request.Message,
            SentAt = DateTime.UtcNow
        };

        _context.InquiryMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        var recipientId = isBuyer ? inquiry.Listing.SellerId : inquiry.BuyerId;
        await _notificationService.CreateAsync(
            recipientId,
            "New Message",
            $"You have a new message regarding '{inquiry.Listing.Title}'.",
            cancellationToken);

        return new InquiryMessageResponseDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = sender?.FullName ?? string.Empty,
            SenderRole = sender?.Role.ToString() ?? string.Empty,
            Message = message.Message,
            SentAt = message.SentAt
        };
    }

    public async Task<InquiryResponseDto> GetInquiryByIdAsync(
        Guid inquiryId, Guid requesterId,
        CancellationToken cancellationToken = default)
    {
        var inquiry = await _context.Inquiries
            .Include(i => i.Listing)
                .ThenInclude(l => l.Images)
            .Include(i => i.Buyer)
            .Include(i => i.Messages)
                .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(i => i.Id == inquiryId, cancellationToken);

        if (inquiry == null)
            throw new KeyNotFoundException("Inquiry not found.");

        var isBuyer = inquiry.BuyerId == requesterId;
        var isSeller = inquiry.Listing.SellerId == requesterId;

        if (!isBuyer && !isSeller)
            throw new UnauthorizedAccessException("You are not part of this inquiry.");

        return MapToDto(inquiry);
    }

    public async Task<List<InquiryResponseDto>> GetInquiriesForListingAsync(
        Guid listingId, Guid sellerId,
        CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
            throw new KeyNotFoundException("Listing not found.");

        if (listing.SellerId != sellerId)
            throw new UnauthorizedAccessException("You do not own this listing.");

        var inquiries = await _context.Inquiries
            .Include(i => i.Listing)
                .ThenInclude(l => l.Images)
            .Include(i => i.Buyer)
            .Include(i => i.Messages)
                .ThenInclude(m => m.Sender)
            .Where(i => i.ListingId == listingId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

        return inquiries.Select(MapToDto).ToList();
    }

    public async Task<List<InquiryResponseDto>> GetInquiriesForBuyerAsync(
        Guid buyerId,
        CancellationToken cancellationToken = default)
    {
        var inquiries = await _context.Inquiries
            .Include(i => i.Listing)
                .ThenInclude(l => l.Images)
            .Include(i => i.Buyer)
            .Include(i => i.Messages)
                .ThenInclude(m => m.Sender)
            .Where(i => i.BuyerId == buyerId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

        return inquiries.Select(MapToDto).ToList();
    }

    private static InquiryResponseDto MapToDto(Inquiry inquiry) => new()
    {
        Id = inquiry.Id,
        ListingId = inquiry.ListingId,
        ListingTitle = inquiry.Listing?.Title ?? string.Empty,
        ListingPrice = inquiry.Listing?.Price ?? 0,
        ListingMainImageUrl = inquiry.Listing?.Images?
            .FirstOrDefault(i => i.IsMain)?.ImageUrl
            ?? inquiry.Listing?.Images?.FirstOrDefault()?.ImageUrl,
        BuyerId = inquiry.BuyerId,
        BuyerName = inquiry.Buyer?.FullName ?? string.Empty,
        CreatedAt = inquiry.CreatedAt,
        Messages = inquiry.Messages?
            .OrderBy(m => m.SentAt)
            .Select(m => new InquiryMessageResponseDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = m.Sender?.FullName ?? string.Empty,
                SenderRole = m.Sender?.Role.ToString() ?? string.Empty,
                Message = m.Message,
                SentAt = m.SentAt
            }).ToList() ?? new()
    };
}
