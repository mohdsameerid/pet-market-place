using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Admin;
using PetMarketplace.Application.Interfaces;

namespace PetMarketplace.Server.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IAdminService _adminService;

    public UsersController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>Get all users with optional role filter</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminUserResponseDto>>>> GetAllUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? role = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetAllUsersAsync(pageNumber, pageSize, role, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminUserResponseDto>>.Ok(result));
    }

    /// <summary>Update user profile fields and role</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminUserResponseDto>>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdateUserAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminUserResponseDto>.Ok(result, "User updated."));
    }

    /// <summary>Give a seller the verified badge</summary>
    [HttpPost("{id:guid}/verify-seller")]
    public async Task<ActionResult<ApiResponse<AdminUserResponseDto>>> VerifySeller(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminService.VerifySellerAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminUserResponseDto>.Ok(result, "Seller verified."));
    }

    /// <summary>Ban a user — they cannot login after this</summary>
    [HttpPost("{id:guid}/ban")]
    public async Task<ActionResult<ApiResponse<AdminUserResponseDto>>> BanUser(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminService.BanUserAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminUserResponseDto>.Ok(result, "User banned."));
    }

    /// <summary>Unban a previously banned user</summary>
    [HttpPost("{id:guid}/unban")]
    public async Task<ActionResult<ApiResponse<AdminUserResponseDto>>> UnbanUser(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminService.UnbanUserAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminUserResponseDto>.Ok(result, "User unbanned."));
    }
}
