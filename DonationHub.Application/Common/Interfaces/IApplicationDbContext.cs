namespace DonationHub.Application.Common.Interfaces;

using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Category> Categories { get; }
    DbSet<Campaign> Campaigns { get; }
    DbSet<Donation> Donations { get; }
    DbSet<PaymentAttempt> PaymentAttempts { get; }
    DbSet<ProcessedStripeEvent> ProcessedStripeEvents { get; }
    DbSet<IdempotencyRecord> IdempotencyRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

