using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Admin;

namespace PetMarketplace.Application.Interfaces;

public interface IAdminService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<AdminListingResponseDto>> GetPendingListingsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<AdminListingResponseDto>> GetAllListingsAsync(int pageNumber, int pageSize, string? status, CancellationToken cancellationToken = default);
    Task<AdminListingResponseDto> ApproveListingAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<AdminListingResponseDto> RejectListingAsync(Guid listingId, RejectListingRequestDto request, CancellationToken cancellationToken = default);
    Task<PagedResult<AdminUserResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize, string? role, CancellationToken cancellationToken = default);
    Task<AdminUserResponseDto> VerifySellerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AdminUserResponseDto> BanUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AdminUserResponseDto> UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
