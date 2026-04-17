using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Auth;
using PetMarketplace.Application.Interfaces;
using PetMarketplace.Core.Entities;
using PetMarketplace.Core.Enums;
using PetMarketplace.Infrastructure.Persistence;

namespace PetMarketplace.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<string> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

        if (exists)
            throw new InvalidOperationException("Email is already registered.");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role) || role == UserRole.Admin)
            throw new InvalidOperationException("Invalid role. Only Buyer or Seller allowed.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            City = request.City,
            Role = role,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return "Registration successful. Please verify your email before logging in.";
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.IsBanned)
            throw new UnauthorizedAccessException("Your account has been banned.");

        if (!user.IsVerified)
            throw new UnauthorizedAccessException("Your email is not verified. Please verify your email before logging in.");

        return BuildAuthResponse(user);
    }

    public async Task<UserProfileDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            ProfileImageUrl = user.ProfileImageUrl,
            Role = user.Role.ToString(),
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(
        Guid userId, UpdateProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.City = request.City;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            ProfileImageUrl = user.ProfileImageUrl,
            Role = user.Role.ToString(),
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt
        };
    }

    private AuthResponseDto BuildAuthResponse(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.ToString(),
        IsVerified = user.IsVerified,
        Token = GenerateJwtToken(user)
    };

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jwtSettings.ExpiryDays),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
