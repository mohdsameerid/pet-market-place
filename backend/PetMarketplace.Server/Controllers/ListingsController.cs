using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Listings;
using PetMarketplace.Application.Interfaces;

namespace PetMarketplace.Server.Controllers;

[ApiController]
[Route("api/listings")]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;
    private readonly ICurrentUserService _currentUser;

    public ListingsController(IListingService listingService, ICurrentUserService currentUser)
    {
        _listingService = listingService;
        _currentUser = currentUser;
    }

    /// <summary>Get all active listings with filters and pagination</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ListingResponseDto>>>> GetAll(
        [FromQuery] ListingFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _listingService.GetAllAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedResult<ListingResponseDto>>.Ok(result));
    }

    /// <summary>Get listing by ID — increments view count</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ListingResponseDto>>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _listingService.GetByIdAsync(id, cancellationToken);
        await _listingService.IncrementViewCountAsync(id, cancellationToken);
        return Ok(ApiResponse<ListingResponseDto>.Ok(result));
    }

    /// <summary>Get current seller's own listings</summary>
    [Authorize(Roles = "Seller")]
    [HttpGet("my-listings")]
    public async Task<ActionResult<ApiResponse<PagedResult<ListingResponseDto>>>> GetMyListings(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var result = await _listingService.GetMyListingsAsync(_currentUser.UserId, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ListingResponseDto>>.Ok(result));
    }

    /// <summary>Create a new listing — Seller only, starts as Draft</summary>
    [Authorize(Roles = "Seller")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ListingResponseDto>>> Create(
        CreateListingRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _listingService.CreateAsync(_currentUser.UserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ListingResponseDto>.Ok(result, "Listing created as Draft."));
    }

    /// <summary>Update an existing listing — Seller only, cannot edit Active/Sold</summary>
    [Authorize(Roles = "Seller")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ListingResponseDto>>> Update(
        Guid id, UpdateListingRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _listingService.UpdateAsync(id, _currentUser.UserId, request, cancellationToken);
        return Ok(ApiResponse<ListingResponseDto>.Ok(result, "Listing updated."));
    }

    /// <summary>Delete a listing — Seller (own) or Admin</summary>
    [Authorize(Roles = "Seller,Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        Guid id, CancellationToken cancellationToken)
    {
        await _listingService.DeleteAsync(id, _currentUser.UserId, _currentUser.Role, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Listing deleted."));
    }

    /// <summary>Submit listing for admin approval — moves Draft/Rejected → PendingApproval</summary>
    [Authorize(Roles = "Seller")]
    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<ApiResponse<ListingResponseDto>>> Submit(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _listingService.SubmitForApprovalAsync(id, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<ListingResponseDto>.Ok(result, "Listing submitted for approval."));
    }
}
