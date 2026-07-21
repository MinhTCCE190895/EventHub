using BLL.Interfaces;
using BLL.DTOs;
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
        // Sử dụng SingleOrDefaultAsync thay vì FirstOrDefaultAsync để đảm bảo tính toàn vẹn dữ liệu (chỉ có duy nhất 1 bản ghi email trong DB).
        return await _userRepo.SingleOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        // Tối ưu tốc độ kiểm tra trùng lặp email ở DB bằng ExistsAsync (chỉ sinh câu lệnh IF EXISTS trong SQL) thay vì load toàn bộ Entity.
        return await _userRepo.ExistsAsync(u => u.Email == email);
    }

    public async Task<User> RegisterAsync(RegisterDto dto)
    {
        _logger.LogInformation("Registering new user {Email} with role {Role}", dto.Email, dto.Role);

        // Khởi tạo Entity với các thông tin mặc định. Quản lý ID từ phía Application thay vì phó mặc cho DB để tiện lợi hơn cho CQRS/Event Sourcing.
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            StudentCode = dto.StudentCode,
            Role = dto.Role,
            // Hash mật khẩu 1 chiều bằng BCrypt. Tham số work factor của bcrypt mặc định là 11 (cân bằng giữa bảo mật và hiệu suất).
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Lưu entity vào DB thông qua Repository Pattern và commit bằng DbContext
        await _userRepo.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {Email} registered with Id {Id}", user.Email, user.Id);
        return user;
    }

    public async Task<User?> ValidateLoginAsync(string email, string password)
    {
        _logger.LogInformation("Login attempt for {Email}", email);

        // Bước 1: Tra cứu User trong hệ thống dựa trên email
        var user = await _userRepo.SingleOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            _logger.LogWarning("Login failed — email {Email} not found", email);
            return null;
        }

        // Bước 2: Chặn đăng nhập nếu tài khoản đã bị vô hiệu hóa (IsActive = false)
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed — account {Email} is locked", email);
            return null;
        }

        try
        {
            // Bước 3: Xác thực tính nguyên vẹn của Hash trước khi Verify
            // Ngăn BCrypt.Verify throw SaltParseException khi gặp hash cũ hoặc sai định dạng.
            if (string.IsNullOrEmpty(user.PasswordHash) || !user.PasswordHash.StartsWith("$2"))
            {
                _logger.LogWarning("Login failed — invalid password hash format for {Email}", email);
                return null;
            }

            // Bước 4: So khớp mật khẩu bản rõ với Hash trong cơ sở dữ liệu
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed — wrong password for {Email}", email);
                return null;
            }
        }
        catch (Exception ex)
        {
            // Ghi log lỗi hệ thống khi Verify thất bại (do lỗi thuật toán/lib) thay vì throw Exception ra controller
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