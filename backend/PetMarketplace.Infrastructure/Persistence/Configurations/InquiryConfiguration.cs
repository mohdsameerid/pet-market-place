using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetMarketplace.Core.Entities;

namespace PetMarketplace.Infrastructure.Persistence.Configurations;

public class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.HasKey(i => i.Id);

        builder.HasOne(i => i.Buyer)
            .WithMany(u => u.Inquiries)
            .HasForeignKey(i => i.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Messages)
            .WithOne(m => m.Inquiry)
            .HasForeignKey(m => m.InquiryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
