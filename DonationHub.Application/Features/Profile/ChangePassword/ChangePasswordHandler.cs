namespace DonationHub.Application.Features.Profile.ChangePassword;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Common.Results;
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
            return Error.Validation("Profile.InvalidUserId", "User ID is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.CurrentPassword))
        {
            return Error.Validation("Profile.CurrentPasswordRequired", "Current password is required.");
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword))
        {
            return Error.Validation("Profile.NewPasswordRequired", "New password is required.");
        }

        if (command.NewPassword.Length < 8)
        {
            return Error.Validation("Profile.PasswordTooShort", "New password must be at least 8 characters long.");
        }

        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("Profile.UserNotFound", "User not found.");
        }

        if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash))
        {
            return Error.Validation("Profile.InvalidCurrentPassword", "The current password is incorrect.");
        }

        var newHash = _passwordHasher.HashPassword(command.NewPassword);
        try
        {
            user.UpdatePasswordHash(newHash);
        }
        catch (DomainException ex)
        {
            return Error.Validation("Profile.DomainError", ex.Message);
        }

        _userRepository.Update(user);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
