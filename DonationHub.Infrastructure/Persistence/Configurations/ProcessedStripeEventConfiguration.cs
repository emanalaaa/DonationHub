namespace DonationHub.Infrastructure.Persistence.Configurations;

using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProcessedStripeEventConfiguration : IEntityTypeConfiguration<ProcessedStripeEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedStripeEvent> builder)
    {
        builder.ToTable("ProcessedStripeEvents");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventId)
            .HasMaxLength(256);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.ProcessedAt)
            .IsRequired();
    }
}

