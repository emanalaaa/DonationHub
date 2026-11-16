namespace DonationHub.Infrastructure.Persistence.Configurations;

using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TitleAr)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(c => c.TitleEn)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(c => c.DescriptionAr)
            .HasMaxLength(4000);

        builder.Property(c => c.DescriptionEn)
            .HasMaxLength(4000);

        builder.Property(c => c.BannerFileName)
            .HasMaxLength(256);

        builder.Property(c => c.Status)
            .IsRequired();

        builder.Property(c => c.EndDate)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.IsDeleted)
            .IsRequired();

        builder.Property(c => c.DeletedAtUtc);

        // Concurrency Token
        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        // Money Value Objects & Decimal Precision
        builder.ComplexProperty(c => c.GoalAmount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("GoalAmount");

            moneyBuilder.Property(m => m.Currency)
                .HasMaxLength(3)
                .HasColumnName("GoalCurrency");
        });

        builder.ComplexProperty(c => c.RaisedAmount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("RaisedAmount");

            moneyBuilder.Property(m => m.Currency)
                .HasMaxLength(3)
                .HasColumnName("RaisedCurrency");
        });

        // Relationships
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

