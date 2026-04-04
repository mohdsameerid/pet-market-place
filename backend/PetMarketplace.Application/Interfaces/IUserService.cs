using PetMarketplace.Application.DTOs.Auth;

namespace PetMarketplace.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> VerifyUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileDto> BanUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileDto> UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
