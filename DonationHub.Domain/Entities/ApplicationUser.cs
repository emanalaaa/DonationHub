namespace DonationHub.Domain.Entities;

using DonationHub.Domain.Exceptions;

public class ApplicationUser
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = "Donor";
    public string PreferredLanguage { get; private set; } = "en";
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ApplicationUser() { }

    public ApplicationUser(
        Guid id,
        string email,
        string userName,
        string fullName,
        string passwordHash,
        DateTime createdAtUtc,
        string role = "Donor",
        string preferredLanguage = "en",
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new DomainException("Username cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("Full name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash cannot be empty.");
        }

        var lang = preferredLanguage.ToLowerInvariant().Trim();
        if (lang != "en" && lang != "ar")
        {
            throw new DomainException("Preferred language must be either 'en' or 'ar'.");
        }

        Id = id;
        Email = email.Trim().ToLowerInvariant();
        UserName = userName.Trim();
        FullName = fullName.Trim();
        PasswordHash = passwordHash;
        Role = string.IsNullOrWhiteSpace(role) ? "Donor" : role.Trim();
        PreferredLanguage = lang;
        IsActive = isActive;
        CreatedAt = createdAtUtc;
    }

    public void UpdateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("Full name cannot be empty.");
        }

        FullName = fullName.Trim();
    }

    public void UpdateProfile(string fullName, string preferredLanguage)
    {
        UpdateFullName(fullName);
        UpdatePreferredLanguage(preferredLanguage);
    }

    public void UpdatePreferredLanguage(string language)
    {
        var lang = language?.ToLowerInvariant().Trim();
        if (lang != "en" && lang != "ar")
        {
            throw new DomainException("Preferred language must be either 'en' or 'ar'.");
        }

        PreferredLanguage = lang;
    }

    public void UpdatePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new DomainException("Password hash cannot be empty.");
        }

        PasswordHash = newPasswordHash;
    }

    public void UpdateRole(string newRole)
    {
        if (string.IsNullOrWhiteSpace(newRole))
        {
            throw new DomainException("Role cannot be empty.");
        }

        Role = newRole.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
