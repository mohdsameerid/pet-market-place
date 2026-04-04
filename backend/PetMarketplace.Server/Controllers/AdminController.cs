using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Auth;
using PetMarketplace.Application.Interfaces;

namespace PetMarketplace.Server.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Verify a user's email — Admin only</summary>
    [HttpPatch("{id:guid}/verify")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> VerifyUser(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _userService.VerifyUserAsync(id, cancellationToken);
        return Ok(ApiResponse<UserProfileDto>.Ok(result, "User verified successfully."));
    }

    /// <summary>Ban a user — Admin only</summary>
    [HttpPatch("{id:guid}/ban")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> BanUser(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _userService.BanUserAsync(id, cancellationToken);
        return Ok(ApiResponse<UserProfileDto>.Ok(result, "User banned successfully."));
    }

    /// <summary>Unban a user — Admin only</summary>
    [HttpPatch("{id:guid}/unban")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UnbanUser(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _userService.UnbanUserAsync(id, cancellationToken);
        return Ok(ApiResponse<UserProfileDto>.Ok(result, "User unbanned successfully."));
    }
}
