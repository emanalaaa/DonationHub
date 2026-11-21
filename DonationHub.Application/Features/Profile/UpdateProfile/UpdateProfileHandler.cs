namespace DonationHub.Application.Features.Profile.UpdateProfile;

using DonationHub.Application.Common.Interfaces;
using DonationHub.Domain.Common.Results;
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
            return Error.Validation("Profile.InvalidUserId", "User ID is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.FullName))
        {
            return Error.Validation("Profile.FullNameRequired", "Full name is required.");
        }

        var lang = command.PreferredLanguage?.Trim().ToLowerInvariant();
        if (lang != "en" && lang != "ar")
        {
            return Error.Validation("Profile.InvalidLanguage", "Preferred language must be either 'en' or 'ar'.");
        }

        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("Profile.UserNotFound", "User not found.");
        }

        try
        {
            user.UpdateProfile(command.FullName, lang);
        }
        catch (DomainException ex)
        {
            return Error.Validation("Profile.DomainError", ex.Message);
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
