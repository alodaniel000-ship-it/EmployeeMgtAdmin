namespace EmployeeMgtAdmin.Domain.Entities;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // Admin, HR, User
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   
}