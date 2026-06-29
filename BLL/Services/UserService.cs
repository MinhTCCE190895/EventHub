using BLL.Interfaces;
using BusinessObjects.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepo;
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(IRepository<User> userRepo, AppDbContext context, ILogger<UserService> logger)
    {
        _userRepo = userRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userRepo.SingleOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _userRepo.ExistsAsync(u => u.Email == email);
    }

    public async Task<User> RegisterAsync(RegisterDto dto)
    {
        _logger.LogInformation("Registering new user {Email} with role {Role}", dto.Email, dto.Role);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            StudentCode = dto.StudentCode,
            Role = dto.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Tham số work factor của bcrypt mặc định là 11
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {Email} registered with Id {Id}", user.Email, user.Id);
        return user;
    }

    public async Task<User?> ValidateLoginAsync(string email, string password)
    {
        _logger.LogInformation("Login attempt for {Email}", email);

        var user = await _userRepo.SingleOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            _logger.LogWarning("Login failed — email {Email} not found", email);
            return null;
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed — account {Email} is locked", email);
            return null;
        }

        try
        {
            // Ngăn BCrypt.Verify throw SaltParseException khi gặp hash cũ hoặc sai định dạng.
            if (string.IsNullOrEmpty(user.PasswordHash) || !user.PasswordHash.StartsWith("$2"))
            {
                _logger.LogWarning("Login failed — invalid password hash format for {Email}", email);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed — wrong password for {Email}", email);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed — error verifying password for {Email}", email);
            return null;
        }

        return user;
    }

    public async Task DeleteUserAsync(Guid id, bool softDelete = true)
    {
        var user = await _userRepo.Query()
            .Include(u => u.OrganizedEvents)
            .Include(u => u.Bookings)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            throw new KeyNotFoundException("Tài khoản không tồn tại.");

        if (softDelete)
        {
            user.IsActive = false;
            _userRepo.Update(user);
            await _context.SaveChangesAsync();
            return;
        }

        if (user.Role == "Organizer" && user.OrganizedEvents.Any())
            throw new InvalidOperationException("Không cho xóa Organizer do đã tạo Sự kiện. Vui lòng sử dụng Xóa mềm (khóa tài khoản).");

        if (user.Role == "Student" && user.Bookings.Any())
            throw new InvalidOperationException("Không cho xóa Student do đang giữ vé đăng ký. Vui lòng sử dụng Xóa mềm (khóa tài khoản).");

        try
        {
            _userRepo.Remove(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Không thể xóa tài khoản do ràng buộc dữ liệu. Vui lòng sử dụng Xóa mềm (khóa tài khoản).");
        }
    }
}
