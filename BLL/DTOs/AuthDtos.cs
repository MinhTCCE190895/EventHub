namespace BLL.DTOs;

public class StudentRegistrationDto
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string StudentCode { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}

public class AdminUserCreateDto
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? StudentCode { get; set; }
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
}

public class AuthenticatedUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}

public class ServiceResultDto
{
    public bool Success { get; set; }
    public string? ErrorField { get; set; }
    public string? ErrorMessage { get; set; }

    public static ServiceResultDto Succeeded()
    {
        return new ServiceResultDto { Success = true };
    }

    public static ServiceResultDto Failed(string message, string? field = null)
    {
        return new ServiceResultDto
        {
            Success = false,
            ErrorField = field,
            ErrorMessage = message
        };
    }
}
