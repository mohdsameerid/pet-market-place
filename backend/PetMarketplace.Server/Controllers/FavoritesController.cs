using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Favorites;
using PetMarketplace.Application.Interfaces;

namespace PetMarketplace.Server.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;
    private readonly ICurrentUserService _currentUser;

    public FavoritesController(IFavoriteService favoriteService, ICurrentUserService currentUser)
    {
        _favoriteService = favoriteService;
        _currentUser = currentUser;
    }

    /// <summary>Add a listing to favorites</summary>
    [HttpPost("{listingId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Add(
        Guid listingId, CancellationToken cancellationToken)
    {
        await _favoriteService.AddAsync(_currentUser.UserId, listingId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Added to favorites."));
    }

    /// <summary>Remove a listing from favorites</summary>
    [HttpDelete("{listingId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Remove(
        Guid listingId, CancellationToken cancellationToken)
    {
        await _favoriteService.RemoveAsync(_currentUser.UserId, listingId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Removed from favorites."));
    }

    /// <summary>Get current user's favorited listings</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<FavoriteResponseDto>>>> GetMyFavorites(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var result = await _favoriteService.GetUserFavoritesAsync(
            _currentUser.UserId, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<FavoriteResponseDto>>.Ok(result));
    }

    /// <summary>Check if a listing is favorited by current user</summary>
    [HttpGet("{listingId:guid}/check")]
    public async Task<ActionResult<ApiResponse<bool>>> Check(
        Guid listingId, CancellationToken cancellationToken)
    {
        var result = await _favoriteService.IsFavoritedAsync(
            _currentUser.UserId, listingId, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result));
    }
}
