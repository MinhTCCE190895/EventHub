using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography;
using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer;
    private readonly IMemoryCache _cache;

    public UserService(
        AppDbContext context,
        ILogger<UserService> logger,
        IEmailSender emailSender,
        IEmailTemplateRenderer emailTemplateRenderer,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _emailSender = emailSender;
        _emailTemplateRenderer = emailTemplateRenderer;
        _cache = cache;
    }

    public Task<AuthenticatedUserDto?> GetAuthenticatedUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Users
            .AsNoTracking()
            .Where(user => user.Id == id && user.IsActive)
            .Select(user => new AuthenticatedUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResultDto> RegisterStudentAsync(
        StudentRegistrationDto dto,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(dto.FullName))
            return ServiceResultDto.Failed("Họ tên không được để trống.", "FullName");

        if (!new EmailAddressAttribute().IsValid(normalizedEmail))
            return ServiceResultDto.Failed("Email không hợp lệ.", "Email");

        if (string.IsNullOrWhiteSpace(dto.StudentCode))
            return ServiceResultDto.Failed("Mã số sinh viên là bắt buộc.", "StudentCode");

        if (dto.Password.Length < 6)
            return ServiceResultDto.Failed("Mật khẩu phải có ít nhất 6 ký tự.", "Password");

        if (!string.Equals(dto.Password, dto.ConfirmPassword, StringComparison.Ordinal))
            return ServiceResultDto.Failed("Mật khẩu xác nhận không khớp.", "ConfirmPassword");

        if (await _context.Users.AsNoTracking()
                .AnyAsync(user => user.Email == normalizedEmail, cancellationToken))
        {
            return ServiceResultDto.Failed("Email này đã được sử dụng.", "Email");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName.Trim(),
            Email = normalizedEmail,
            StudentCode = dto.StudentCode.Trim(),
            Role = ApplicationRoles.Student,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New student registered with Id {UserId}", user.Id);
        return ServiceResultDto.Succeeded();
    }

    public async Task<AuthenticatedUserDto?> ValidateLoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Email == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            _logger.LogWarning("Login failed for {Email}", normalizedEmail);
            return null;
        }

        try
        {
            if (string.IsNullOrEmpty(user.PasswordHash) ||
                !user.PasswordHash.StartsWith("$2", StringComparison.Ordinal) ||
                !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for {Email}", normalizedEmail);
                return null;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Password verification failed for {Email}", normalizedEmail);
            return null;
        }

        return new AuthenticatedUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<ServiceResultDto> SendPasswordResetOtpAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Email == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            _logger.LogWarning("Password reset requested for an unavailable account");
            return ServiceResultDto.Succeeded();
        }

        var otp = RandomNumberGenerator.GetInt32(100000, 1000000)
            .ToString("D6", CultureInfo.InvariantCulture);
        var cacheKey = GetResetOtpCacheKey(normalizedEmail);
        _cache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

        var subject = "[EventHub] Mã xác thực khôi phục mật khẩu";
        try
        {
            var body = await _emailTemplateRenderer.RenderAsync(
                "PasswordResetOtp",
                new Dictionary<string, string>
                {
                    ["FullName"] = user.FullName,
                    ["Otp"] = otp
                },
                cancellationToken);

            await _emailSender.SendEmailAsync(normalizedEmail, subject, body, cancellationToken);
            _logger.LogInformation("Password reset OTP sent");
            return ServiceResultDto.Succeeded();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _cache.Remove(cacheKey);
            throw;
        }
        catch (Exception exception)
        {
            _cache.Remove(cacheKey);
            _logger.LogError(exception, "Unable to prepare or send a password reset email");
            return ServiceResultDto.Failed($"Gửi email thất bại: {exception.Message}");
        }
    }

    public async Task<ServiceResultDto> ResetPasswordWithOtpAsync(
        string email,
        string otp,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (newPassword.Length < 6)
            return ServiceResultDto.Failed("Mật khẩu mới phải có ít nhất 6 ký tự.");

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var cacheKey = GetResetOtpCacheKey(normalizedEmail);
        if (!_cache.TryGetValue(cacheKey, out string? cachedOtp) ||
            !string.Equals(cachedOtp, otp, StringComparison.Ordinal))
        {
            return ServiceResultDto.Failed(
                "Mã OTP không chính xác hoặc đã hết hạn (chỉ có hiệu lực trong 5 phút).");
        }

        var user = await _context.Users.SingleOrDefaultAsync(
            candidate => candidate.Email == normalizedEmail,
            cancellationToken);
        if (user is null || !user.IsActive)
            return ServiceResultDto.Failed("Tài khoản không tồn tại hoặc đã bị khóa.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync(cancellationToken);

        _cache.Remove(cacheKey);
        _logger.LogInformation("Password reset successfully for User {UserId}", user.Id);
        return ServiceResultDto.Succeeded();
    }

    private static string GetResetOtpCacheKey(string normalizedEmail)
    {
        return $"ResetOtp_{normalizedEmail}";
    }
}
