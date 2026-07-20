using BLL.DTOs;
using DAL.Entities;

namespace BLL.Interfaces;

public interface IUserService
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<User> RegisterAsync(RegisterDto dto);
    /// <summary>
    /// Validate login — trả về User nếu thành công, null nếu sai email/pass hoặc bị khoá.
    /// </summary>
    Task<User?> ValidateLoginAsync(string email, string password);
    Task DeleteUserAsync(Guid id, bool softDelete = true);
}