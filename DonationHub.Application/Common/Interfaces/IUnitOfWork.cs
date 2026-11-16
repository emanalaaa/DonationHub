namespace DonationHub.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}

