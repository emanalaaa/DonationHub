namespace DonationHub.Application.Features.Profile.ChangePassword;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Common.Models;
using DonationHub.Domain.Entities;
using DonationHub.Domain.Exceptions;

public class ChangePasswordHandler
{
    private readonly IGenericRepository<ApplicationUser> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordHandler(
        IGenericRepository<ApplicationUser> userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        if (command.UserId == Guid.Empty)
        {
            return Result.Failure("Profile.InvalidUserId", "User ID is invalid.", 400);
        }

        if (string.IsNullOrWhiteSpace(command.CurrentPassword))
        {
            return Result.Failure("Profile.CurrentPasswordRequired", "Current password is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword))
        {
            return Result.Failure("Profile.NewPasswordRequired", "New password is required.", 400);
        }

        if (command.NewPassword.Length < 8)
        {
            return Result.Failure("Profile.PasswordTooShort", "New password must be at least 8 characters long.", 400);
        }

        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("Profile.UserNotFound", "User not found.", 404);
        }

        if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure("Profile.InvalidCurrentPassword", "The current password is incorrect.", 400);
        }

        var newHash = _passwordHasher.HashPassword(command.NewPassword);
        try
        {
            user.UpdatePasswordHash(newHash);
        }
        catch (DomainException ex)
        {
            return Result.Failure("Profile.DomainError", ex.Message, 400);
        }

        _userRepository.Update(user);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

