namespace DonationHub.Application.Features.Profile.UpdateProfile;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Application.Common.Models;
using DonationHub.Domain.Entities;
using DonationHub.Domain.Exceptions;

public class UpdateProfileHandler
{
    private readonly IGenericRepository<ApplicationUser> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileHandler(
        IGenericRepository<ApplicationUser> userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserProfileDto>> HandleAsync(UpdateProfileCommand command, CancellationToken cancellationToken = default)
    {
        if (command.UserId == Guid.Empty)
        {
            return Result<UserProfileDto>.Failure("Profile.InvalidUserId", "User ID is invalid.", 400);
        }

        if (string.IsNullOrWhiteSpace(command.FullName))
        {
            return Result<UserProfileDto>.Failure("Profile.FullNameRequired", "Full name is required.", 400);
        }

        var lang = command.PreferredLanguage?.Trim().ToLowerInvariant();
        if (lang != "en" && lang != "ar")
        {
            return Result<UserProfileDto>.Failure("Profile.InvalidLanguage", "Preferred language must be either 'en' or 'ar'.", 400);
        }

        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<UserProfileDto>.Failure("Profile.UserNotFound", "User not found.", 404);
        }

        try
        {
            user.UpdateProfile(command.FullName, lang);
        }
        catch (DomainException ex)
        {
            return Result<UserProfileDto>.Failure("Profile.DomainError", ex.Message, 400);
        }

        _userRepository.Update(user);
        await _unitOfWork.CompleteAsync(cancellationToken);

        var dto = new UserProfileDto(
            user.Id,
            user.Email,
            user.UserName,
            user.FullName,
            user.Role,
            user.PreferredLanguage,
            user.IsActive);

        return Result<UserProfileDto>.Success(dto);
    }
}

