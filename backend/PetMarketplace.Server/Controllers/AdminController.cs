using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Admin;
using PetMarketplace.Application.Interfaces;

namespace PetMarketplace.Server.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>Get dashboard stats — total users, listings by status, inquiries</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboard(
        CancellationToken cancellationToken)
    { 
        var result = await _adminService.GetDashboardStatsAsync(cancellationToken);
        return Ok(ApiResponse<DashboardStatsDto>.Ok(result));
    }

    /// <summary>Get all pending listings waiting for approval</summary>
    [HttpGet("listings/pending")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminListingResponseDto>>>> GetPendingListings(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetPendingListingsAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminListingResponseDto>>.Ok(result));
    }

    /// <summary>Get all listings with optional status filter</summary>
    [HttpGet("listings")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminListingResponseDto>>>> GetAllListings(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetAllListingsAsync(pageNumber, pageSize, status, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminListingResponseDto>>.Ok(result));
    }

    /// <summary>Approve a pending listing — makes it Active and visible publicly</summary>
    [HttpPost("listings/{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse<AdminListingResponseDto>>> ApproveListing(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminService.ApproveListingAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminListingResponseDto>.Ok(result, "Listing approved."));
    }

    /// <summary>Reject a pending listing with a reason</summary>
    [HttpPost("listings/{id:guid}/reject")]
    public async Task<ActionResult<ApiResponse<AdminListingResponseDto>>> RejectListing(
        Guid id,
        RejectListingRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.RejectListingAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminListingResponseDto>.Ok(result, "Listing rejected."));
    }

}
