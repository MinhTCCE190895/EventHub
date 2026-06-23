using BLL.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
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

        // Khởi tạo Entity User mới với các thông tin cơ bản
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            StudentCode = dto.StudentCode,
            Role = dto.Role,
            // Mã hóa mật khẩu một chiều bằng BCrypt để đảm bảo an toàn nếu lộ database
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true, // Mặc định tài khoản mới tạo được phép hoạt động
            CreatedAt = DateTime.UtcNow
        };

        // Lưu vào cơ sở dữ liệu thông qua UnitOfWork/Repository pattern
        await _userRepo.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {Email} registered with Id {Id}", user.Email, user.Id);
        return user;
    }

    public async Task<User?> ValidateLoginAsync(string email, string password)
    {
        _logger.LogInformation("Login attempt for {Email}", email);

        // 1. Tìm user theo Email trong Database
        var user = await _userRepo.SingleOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            _logger.LogWarning("Login failed — email {Email} not found", email);
            return null;
        }

        // 2. Kiểm tra trạng thái hoạt động (bị khóa bởi Admin)
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed — account {Email} is locked", email);
            return null;
        }

        // 3. Xác thực mật khẩu bằng BCrypt
        try
        {
            // Bảo vệ ứng dụng khỏi crash (throw Exception) khi verify các hash lỗi hoặc cũ không phải chuẩn BCrypt
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

        // Đăng nhập thành công, trả về Entity User
        return user;
    }
}
