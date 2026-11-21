namespace DonationHub.Application.Features.Admin.CreateStaff;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Common.Errors;
using DonationHub.Domain.Common.Results;
using DonationHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, Result<Guid>>
{
    private readonly IGenericRepository<ApplicationUser> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;

    public CreateStaffCommandHandler(
        IGenericRepository<ApplicationUser> userRepository,
        IUnitOfWork unitOfWork,
        IAppDbContext context,
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _context = context;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Guid>> Handle(
        CreateStaffCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            return AuthErrors.EmailAlreadyExists;
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var staffUser = new ApplicationUser(
            Guid.NewGuid(),
            normalizedEmail,
            request.UserName.Trim(),
            request.FullName.Trim(),
            passwordHash,
            now,
            role: request.Role.Trim(),
            preferredLanguage: string.IsNullOrWhiteSpace(request.PreferredLanguage) ? "en" : request.PreferredLanguage);

        await _userRepository.AddAsync(staffUser, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return staffUser.Id;
    }
}

