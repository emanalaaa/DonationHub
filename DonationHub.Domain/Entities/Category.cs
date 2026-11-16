namespace DonationHub.Domain.Entities;

using DonationHub.Domain.Exceptions;

public class Category
{
    public Guid Id { get; private set; }
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string DescriptionAr { get; private set; } = string.Empty;
    public string DescriptionEn { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Category() { }

    public Category(
        Guid id,
        string nameAr,
        string nameEn,
        string descriptionAr,
        string descriptionEn,
        DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
        {
            throw new DomainException("Arabic name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(nameEn))
        {
            throw new DomainException("English name cannot be empty.");
        }

        Id = id;
        NameAr = nameAr.Trim();
        NameEn = nameEn.Trim();
        DescriptionAr = descriptionAr?.Trim() ?? string.Empty;
        DescriptionEn = descriptionEn?.Trim() ?? string.Empty;
        CreatedAt = createdAtUtc;
    }

    public void Update(
        string nameAr,
        string nameEn,
        string descriptionAr,
        string descriptionEn,
        DateTime updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
        {
            throw new DomainException("Arabic name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(nameEn))
        {
            throw new DomainException("English name cannot be empty.");
        }

        NameAr = nameAr.Trim();
        NameEn = nameEn.Trim();
        DescriptionAr = descriptionAr?.Trim() ?? string.Empty;
        DescriptionEn = descriptionEn?.Trim() ?? string.Empty;
        UpdatedAt = updatedAtUtc;
    }
}

