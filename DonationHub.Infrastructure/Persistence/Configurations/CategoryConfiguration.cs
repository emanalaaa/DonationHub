namespace DonationHub.Infrastructure.Persistence.Configurations;

using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.NameAr)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.NameAr)
            .IsUnique();

        builder.Property(c => c.NameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.NameEn)
            .IsUnique();

        builder.Property(c => c.DescriptionAr)
            .HasMaxLength(1000);

        builder.Property(c => c.DescriptionEn)
            .HasMaxLength(1000);

        builder.Property(c => c.CreatedAt)
            .IsRequired();
    }
}
