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
        // S? d?ng SingleOrDefaultAsync thay vì FirstOrDefaultAsync d? d?m b?o tính toàn v?n d? li?u (ch? có duy nh?t 1 b?n ghi email trong DB).
        return await _userRepo.SingleOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        // T?i uu t?c d? ki?m tra trùng l?p email ? DB b?ng ExistsAsync (ch? sinh câu l?nh IF EXISTS trong SQL) thay vì load toàn b? Entity.
        return await _userRepo.ExistsAsync(u => u.Email == email);
    }

    public async Task<User> RegisterAsync(RegisterDto dto)
    {
        _logger.LogInformation("Registering new user {Email} with role {Role}", dto.Email, dto.Role);

        // Kh?i t?o Entity v?i các thông tin m?c d?nh. Qu?n lý ID t? phía Application thay vì phó m?c cho DB d? ti?n l?i hon cho CQRS/Event Sourcing.
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            StudentCode = dto.StudentCode,
            Role = dto.Role,
            // Hash m?t kh?u 1 chi?u b?ng BCrypt. Tham s? work factor c?a bcrypt m?c d?nh là 11 (cân b?ng gi?a b?o m?t và hi?u su?t).
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Luu entity vào DB thông qua Repository Pattern và commit b?ng DbContext
        await _userRepo.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {Email} registered with Id {Id}", user.Email, user.Id);
        return user;
    }

    public async Task<User?> ValidateLoginAsync(string email, string password)
    {
        _logger.LogInformation("Login attempt for {Email}", email);

        // Bu?c 1: Tra c?u User trong h? th?ng d?a trên email
        var user = await _userRepo.SingleOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            _logger.LogWarning("Login failed — email {Email} not found", email);
            return null;
        }

        // Bu?c 2: Ch?n dang nh?p n?u tài kho?n dã b? vô hi?u hóa (IsActive = false)
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed — account {Email} is locked", email);
            return null;
        }

        try
        {
            // Bu?c 3: Xác th?c tính nguyên v?n c?a Hash tru?c khi Verify
            // Ngan BCrypt.Verify throw SaltParseException khi g?p hash cu ho?c sai d?nh d?ng.
            if (string.IsNullOrEmpty(user.PasswordHash) || !user.PasswordHash.StartsWith("$2"))
            {
                _logger.LogWarning("Login failed — invalid password hash format for {Email}", email);
                return null;
            }

            // Bu?c 4: So kh?p m?t kh?u b?n rõ v?i Hash trong co s? d? li?u
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed — wrong password for {Email}", email);
                return null;
            }
        }
        catch (Exception ex)
        {
            // Ghi log l?i h? th?ng khi Verify th?t b?i (do l?i thu?t toán/lib) thay vì throw Exception ra controller
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
            throw new KeyNotFoundException("Tài kho?n không t?n t?i.");

        if (softDelete)
        {
            user.IsActive = false;
            _userRepo.Update(user);
            await _context.SaveChangesAsync();
            return;
        }

        if (user.Role == "Organizer" && user.OrganizedEvents.Any())
            throw new InvalidOperationException("Không cho xóa Organizer do dã t?o S? ki?n. Vui lòng s? d?ng Xóa m?m (khóa tài kho?n).");

        if (user.Role == "Student" && user.Bookings.Any())
            throw new InvalidOperationException("Không cho xóa Student do dang gi? vé dang ký. Vui lòng s? d?ng Xóa m?m (khóa tài kho?n).");

        try
        {
            _userRepo.Remove(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Không th? xóa tài kho?n do ràng bu?c d? li?u. Vui lòng s? d?ng Xóa m?m (khóa tài kho?n).");
        }
    }
}
