namespace DonationHub.Infrastructure.Persistence.Configurations;

using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DonationConfiguration : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        builder.ToTable("Donations");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Status)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.FailureReason)
            .HasMaxLength(500);

        builder.Property(d => d.ReceiptNumber)
            .HasMaxLength(100);

        builder.Property(d => d.ReceiptFileName)
            .HasMaxLength(256);

        // Money Value Object & Decimal Precision
        builder.ComplexProperty(d => d.Amount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("Amount");

            moneyBuilder.Property(m => m.Currency)
                .HasMaxLength(3)
                .HasColumnName("Currency");
        });

        // Relationships
        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(d => d.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(d => d.DonorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

