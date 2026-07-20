using BLL.DTOs;
using DAL.Entities;

namespace BLL.Interfaces;

public interface IUserService
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<User> RegisterAsync(RegisterDto dto);
    /// <summary>
    /// Validate login — tr? v? User n?u thành công, null n?u sai email/pass ho?c b? khoá.
    /// </summary>
    Task<User?> ValidateLoginAsync(string email, string password);
    Task DeleteUserAsync(Guid id, bool softDelete = true);
}
