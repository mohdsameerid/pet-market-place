using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetMarketplace.Core.Entities;

namespace PetMarketplace.Infrastructure.Persistence.Configurations;

public class InquiryMessageConfiguration : IEntityTypeConfiguration<InquiryMessage>
{
    public void Configure(EntityTypeBuilder<InquiryMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Message).IsRequired().HasMaxLength(1000);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
