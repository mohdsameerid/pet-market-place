using Microsoft.EntityFrameworkCore;
using PetMarketplace.Application.DTOs.Auth;
using PetMarketplace.Application.Interfaces;
using PetMarketplace.Infrastructure.Persistence;

namespace PetMarketplace.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto> VerifyUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.IsVerified)
            throw new InvalidOperationException("User is already verified.");

        user.IsVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    public async Task<UserProfileDto> BanUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.IsBanned)
            throw new InvalidOperationException("User is already banned.");

        user.IsBanned = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    public async Task<UserProfileDto> UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        if (!user.IsBanned)
            throw new InvalidOperationException("User is not banned.");

        user.IsBanned = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    private static UserProfileDto MapToDto(Core.Entities.User user) => new()
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
