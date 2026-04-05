using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMarketplace.Application.Common;
using PetMarketplace.Application.DTOs.Notifications;
using PetMarketplace.Application.Interfaces;

namespace PetMarketplace.Server.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(
        INotificationService notificationService,
        ICurrentUserService currentUser)
    {
        _notificationService = notificationService;
        _currentUser = currentUser;
    }

    /// <summary>Get current user's notifications — latest 50</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationResponseDto>>>> GetMyNotifications(
        CancellationToken cancellationToken)
    {
        var result = await _notificationService.GetForUserAsync(_currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<List<NotificationResponseDto>>.Ok(result));
    }

    /// <summary>Get unread notification count</summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount(
        CancellationToken cancellationToken)
    {
        var count = await _notificationService.GetUnreadCountAsync(_currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<int>.Ok(count));
    }

    /// <summary>Mark a single notification as read</summary>
    [HttpPut("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(
        Guid id, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsReadAsync(id, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "Marked as read."));
    }

    /// <summary>Mark all notifications as read</summary>
    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead(
        CancellationToken cancellationToken)
    {
        await _notificationService.MarkAllAsReadAsync(_currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "All notifications marked as read."));
    }
}
