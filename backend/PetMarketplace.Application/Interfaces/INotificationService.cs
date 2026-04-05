using PetMarketplace.Application.DTOs.Notifications;

namespace PetMarketplace.Application.Interfaces;

public interface INotificationService
{
    Task CreateAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default);
    Task<List<NotificationResponseDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
