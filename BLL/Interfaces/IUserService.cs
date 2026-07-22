using BLL.DTOs;

namespace BLL.Interfaces;

public interface IUserService
{
    Task<AuthenticatedUserDto?> GetAuthenticatedUserAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task<ServiceResultDto> RegisterStudentAsync(
        StudentRegistrationDto dto,
        CancellationToken cancellationToken = default);
    Task<AuthenticatedUserDto?> ValidateLoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
    Task<ServiceResultDto> SendPasswordResetOtpAsync(
        string email,
        CancellationToken cancellationToken = default);
    Task<ServiceResultDto> ResetPasswordWithOtpAsync(
        string email,
        string otp,
        string newPassword,
        CancellationToken cancellationToken = default);
}
