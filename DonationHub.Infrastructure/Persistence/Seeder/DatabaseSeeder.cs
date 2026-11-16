namespace DonationHub.Infrastructure.Persistence.Seeder;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Entities;
using DonationHub.Infrastructure.Options;
using DonationHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly SeedAdminOptions _adminOptions;
    private readonly bool _isDevelopment;

    public DatabaseSeeder(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        IOptions<SeedAdminOptions> adminOptions,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _adminOptions = adminOptions.Value;
        _isDevelopment = string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Development", StringComparison.OrdinalIgnoreCase);
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // 1. Seed Categories if empty
        if (!await _context.Categories.AnyAsync(cancellationToken))
        {
            var categories = new List<Category>
            {
                new(Guid.NewGuid(), "الصحة والرعاية الطبية", "Health & Medical", "دعم المستشفيات والمشاريع الطبية وتكاليف العلاج", "Supporting hospitals, medical facilities, and critical patient care", DateTime.UtcNow),
                new(Guid.NewGuid(), "التعليم والتدريب", "Education & Training", "بناء المدارس وتوفير المنح الدراسية والمستلزمات التعليمية", "Building schools, funding scholarships, and educational supplies", DateTime.UtcNow),
                new(Guid.NewGuid(), "الإغاثة والطوارئ", "Emergency Relief", "مساعدات عاجلة للمنكوبين في الكوارث والحروب", "Emergency aid, food packages, and shelter for crisis-affected populations", DateTime.UtcNow),
                new(Guid.NewGuid(), "رعاية الأيتام", "Orphan Care", "كفالة الأيتام وتوفير الرعاية المعيشية والتعليمية", "Comprehensive care, living stipends, and schooling for orphans", DateTime.UtcNow),
                new(Guid.NewGuid(), "البيئة والتنمية المستدامة", "Environment & Sustainability", "مشاريع التشجير والمياه النظيفة والطاقة المتجددة", "Clean water access, afforestation, and renewable energy projects", DateTime.UtcNow)
            };

            await _context.Categories.AddRangeAsync(categories, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 2. Seed Admin User
        var adminEmail = _adminOptions.Email.Trim().ToLowerInvariant();
        if (!await _context.Users.AnyAsync(u => u.Email == adminEmail, cancellationToken))
        {
            var adminPasswordHash = _passwordHasher.HashPassword(_adminOptions.Password);
            var adminUser = new ApplicationUser(
                Guid.NewGuid(),
                adminEmail,
                "admin",
                _adminOptions.FullName,
                adminPasswordHash,
                DateTime.UtcNow,
                role: "Admin",
                preferredLanguage: "en");

            await _context.Users.AddAsync(adminUser, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 3. Seed Development-only Users (CampaignManager and Donor)
        if (_isDevelopment)
        {
            const string managerEmail = "manager@donationhub.org";
            if (!await _context.Users.AnyAsync(u => u.Email == managerEmail, cancellationToken))
            {
                var managerHash = _passwordHasher.HashPassword("Manager@12345");
                var managerUser = new ApplicationUser(
                    Guid.NewGuid(),
                    managerEmail,
                    "campaign_manager",
                    "Campaign Manager",
                    managerHash,
                    DateTime.UtcNow,
                    role: "CampaignManager",
                    preferredLanguage: "ar");

                await _context.Users.AddAsync(managerUser, cancellationToken);
            }

            const string donorEmail = "donor@donationhub.org";
            if (!await _context.Users.AnyAsync(u => u.Email == donorEmail, cancellationToken))
            {
                var donorHash = _passwordHasher.HashPassword("Donor@12345");
                var donorUser = new ApplicationUser(
                    Guid.NewGuid(),
                    donorEmail,
                    "sample_donor",
                    "Generous Donor",
                    donorHash,
                    DateTime.UtcNow,
                    role: "Donor",
                    preferredLanguage: "en");

                await _context.Users.AddAsync(donorUser, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
