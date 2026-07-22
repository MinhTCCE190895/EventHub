namespace BLL;

public static class ApplicationRoles
{
    public const string Admin = "Admin";
    public const string Organizer = "Organizer";
    public const string Student = "Student";
    public const string ManageableRolePattern = "^(Student|Organizer)$";

    public static bool IsManageable(string? role)
    {
        return role is Student or Organizer;
    }
}
