namespace PetMarketplace.Core.Entities;

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;

    // FK
    public Guid UserId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
