using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Reviews;
using PetMarketplace.Application.Interfaces;

namespace PetMarketplace.Server.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ICurrentUserService _currentUser;

    public ReviewsController(IReviewService reviewService, ICurrentUserService currentUser)
    {
        _reviewService = reviewService;
        _currentUser = currentUser;
    }

    /// <summary>Leave a review for a seller — one review per seller per buyer</summary>
    [Authorize(Roles = "Buyer")]
    [HttpPost("sellers/{sellerId:guid}")]
    public async Task<ActionResult<ApiResponse<ReviewResponseDto>>> Create(
        Guid sellerId,
        CreateReviewRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _reviewService.CreateAsync(
            _currentUser.UserId, sellerId, request, cancellationToken);
        return Ok(ApiResponse<ReviewResponseDto>.Ok(result, "Review submitted."));
    }

    /// <summary>Get all reviews for a seller with average rating</summary>
    [HttpGet("sellers/{sellerId:guid}")]
    public async Task<ActionResult<ApiResponse<SellerReviewSummaryDto>>> GetSellerReviews(
        Guid sellerId, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetSellerReviewsAsync(sellerId, cancellationToken);
        return Ok(ApiResponse<SellerReviewSummaryDto>.Ok(result));
    }
}
