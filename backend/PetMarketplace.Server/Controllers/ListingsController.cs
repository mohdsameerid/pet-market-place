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
    private readonly IListingImageService _imageService;
    private readonly ICurrentUserService _currentUser;

    public ListingsController(
        IListingService listingService,
        IListingImageService imageService,
        ICurrentUserService currentUser)
    {
        _listingService = listingService;
        _imageService = imageService;
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

    // ── Image endpoints ────────────────────────────────────────────────────────

    /// <summary>Upload image for a listing — max 6 images, first image auto-set as main</summary>
    [Authorize(Roles = "Seller")]
    [HttpPost("{id:guid}/images")]
    public async Task<ActionResult<ApiResponse<UploadImageResponseDto>>> UploadImage(
        Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<UploadImageResponseDto>.Fail("No file provided."));

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(ApiResponse<UploadImageResponseDto>.Fail("Only JPG, PNG, and WEBP images are allowed."));

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(ApiResponse<UploadImageResponseDto>.Fail("Image size must be under 5MB."));

        using var stream = file.OpenReadStream();
        var result = await _imageService.UploadImageAsync(
            id, _currentUser.UserId, stream, file.FileName, cancellationToken);

        return Ok(ApiResponse<UploadImageResponseDto>.Ok(result, "Image uploaded successfully."));
    }

    /// <summary>Delete an image from a listing — also removes from Cloudinary</summary>
    [Authorize(Roles = "Seller")]
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteImage(
        Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        await _imageService.DeleteImageAsync(id, imageId, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Image deleted."));
    }

    /// <summary>Set an image as the main/cover image for a listing</summary>
    [Authorize(Roles = "Seller")]
    [HttpPut("{id:guid}/images/{imageId:guid}/set-main")]
    public async Task<ActionResult<ApiResponse<object>>> SetMainImage(
        Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        await _imageService.SetMainImageAsync(id, imageId, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Main image updated."));
    }
}
