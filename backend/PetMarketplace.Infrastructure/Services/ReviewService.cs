using Microsoft.EntityFrameworkCore;
using PetMarketplace.Application.DTOs.Reviews;
using PetMarketplace.Application.Interfaces;
using PetMarketplace.Core.Entities;
using PetMarketplace.Core.Enums;
using PetMarketplace.Infrastructure.Persistence;

namespace PetMarketplace.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewResponseDto> CreateAsync(
        Guid reviewerId, Guid sellerId,
        CreateReviewRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (reviewerId == sellerId)
            throw new InvalidOperationException("You cannot review yourself.");

        var seller = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == sellerId && u.Role == UserRole.Seller, cancellationToken);

        if (seller == null)
            throw new KeyNotFoundException("Seller not found.");

        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.ReviewerId == reviewerId && r.SellerId == sellerId, cancellationToken);

        if (alreadyReviewed)
            throw new InvalidOperationException("You have already reviewed this seller.");

        var review = new Review
        {
            ReviewerId = reviewerId,
            SellerId = sellerId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        var reviewer = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == reviewerId, cancellationToken);

        return new ReviewResponseDto
        {
            Id = review.Id,
            ReviewerId = review.ReviewerId,
            ReviewerName = reviewer?.FullName ?? string.Empty,
            ReviewerImageUrl = reviewer?.ProfileImageUrl,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<SellerReviewSummaryDto> GetSellerReviewsAsync(
        Guid sellerId, CancellationToken cancellationToken = default)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Reviewer)
            .Where(r => r.SellerId == sellerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return new SellerReviewSummaryDto
        {
            TotalReviews = reviews.Count,
            AverageRating = reviews.Count > 0
                ? Math.Round(reviews.Average(r => r.Rating), 1)
                : 0,
            Reviews = reviews.Select(r => new ReviewResponseDto
            {
                Id = r.Id,
                ReviewerId = r.ReviewerId,
                ReviewerName = r.Reviewer?.FullName ?? string.Empty,
                ReviewerImageUrl = r.Reviewer?.ProfileImageUrl,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        };
    }
}
