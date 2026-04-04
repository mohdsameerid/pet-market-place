using Microsoft.EntityFrameworkCore;
using PetMarketplace.Core.Entities;
using PetMarketplace.Core.Enums;

namespace PetMarketplace.Infrastructure.Persistence;

public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = adminId,
            FullName = "Admin User",
            Email = "admin@petmarketplace.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            IsVerified = true,
            IsBanned = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }
}
