using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Inquiries;
using PetMarketplace.Application.Interfaces;

namespace PetMarketplace.Server.Controllers;

[ApiController]
[Route("api/inquiries")]
[Authorize]
public class InquiriesController : ControllerBase
{
    private readonly IInquiryService _inquiryService;
    private readonly ICurrentUserService _currentUser;

    public InquiriesController(IInquiryService inquiryService, ICurrentUserService currentUser)
    {
        _inquiryService = inquiryService;
        _currentUser = currentUser;
    }

    /// <summary>Create a new inquiry on a listing — Buyer only, one per listing</summary>
    [Authorize(Roles = "Buyer")]
    [HttpPost("listings/{listingId:guid}")]
    public async Task<ActionResult<ApiResponse<InquiryResponseDto>>> CreateInquiry(
        Guid listingId,
        CreateInquiryRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _inquiryService.CreateInquiryAsync(
            _currentUser.UserId, listingId, request, cancellationToken);
        return Ok(ApiResponse<InquiryResponseDto>.Ok(result, "Inquiry created."));
    }

    /// <summary>Send a message in an existing inquiry — Buyer or Seller only</summary>
    [HttpPost("{inquiryId:guid}/messages")]
    public async Task<ActionResult<ApiResponse<InquiryMessageResponseDto>>> SendMessage(
        Guid inquiryId,
        SendMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _inquiryService.SendMessageAsync(
            _currentUser.UserId, inquiryId, request, cancellationToken);
        return Ok(ApiResponse<InquiryMessageResponseDto>.Ok(result, "Message sent."));
    }

    /// <summary>Get a single inquiry with full message thread</summary>
    [HttpGet("{inquiryId:guid}")]
    public async Task<ActionResult<ApiResponse<InquiryResponseDto>>> GetById(
        Guid inquiryId, CancellationToken cancellationToken)
    {
        var result = await _inquiryService.GetInquiryByIdAsync(
            inquiryId, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<InquiryResponseDto>.Ok(result));
    }

    /// <summary>Get all inquiries for a listing — Seller only</summary>
    [Authorize(Roles = "Seller")]
    [HttpGet("listings/{listingId:guid}")]
    public async Task<ActionResult<ApiResponse<List<InquiryResponseDto>>>> GetForListing(
        Guid listingId, CancellationToken cancellationToken)
    {
        var result = await _inquiryService.GetInquiriesForListingAsync(
            listingId, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<List<InquiryResponseDto>>.Ok(result));
    }

    /// <summary>Get current buyer's inquiries across all listings</summary>
    [Authorize(Roles = "Buyer")]
    [HttpGet("my-inquiries")]
    public async Task<ActionResult<ApiResponse<List<InquiryResponseDto>>>> GetMyInquiries(
        CancellationToken cancellationToken)
    {
        var result = await _inquiryService.GetInquiriesForBuyerAsync(
            _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<List<InquiryResponseDto>>.Ok(result));
    }
}
