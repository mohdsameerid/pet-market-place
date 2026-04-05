namespace PetMarketplace.Application.DTOs.Admin;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalBuyers { get; set; }
    public int TotalSellers { get; set; }
    public int TotalListings { get; set; }
    public int DraftListings { get; set; }
    public int PendingListings { get; set; }
    public int ActiveListings { get; set; }
    public int RejectedListings { get; set; }
    public int SoldListings { get; set; }
    public int TotalInquiries { get; set; }
    public int TotalFavorites { get; set; }
    public int VerifiedSellers { get; set; }
    public int BannedUsers { get; set; }
}
