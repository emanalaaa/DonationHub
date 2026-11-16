namespace DonationHub.Infrastructure.Persistence.Configurations;

using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("PaymentAttempts");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.StripeCheckoutSessionId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.StripePaymentIntentId)
            .HasMaxLength(256);

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.FailureCode)
            .HasMaxLength(100);

        // Money Value Object & Decimal Precision
        builder.ComplexProperty(p => p.Amount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("Amount");

            moneyBuilder.Property(m => m.Currency)
                .HasMaxLength(3)
                .HasColumnName("Currency");
        });

        // Relationship
        builder.HasOne<Donation>()
            .WithMany()
            .HasForeignKey(p => p.DonationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

