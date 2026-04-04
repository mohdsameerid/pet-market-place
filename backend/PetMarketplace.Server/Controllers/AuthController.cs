using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Auth;
using PetMarketplace.Application.Interfaces;

namespace PetMarketplace.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    /// <summary>Register a new Buyer or Seller account</summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<string>>> Register(
        RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var message = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok(message));
    }

    /// <summary>Login and receive JWT token</summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
    }

    /// <summary>Get current logged-in user profile</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> Me(CancellationToken cancellationToken)
    {
        var result = await _authService.GetCurrentUserAsync(_currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<UserProfileDto>.Ok(result));
    }
}
