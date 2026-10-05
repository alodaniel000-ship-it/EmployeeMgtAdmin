namespace EmployeeMgtAdmin.Application.DTOs.Employee;

public class UpdateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Guid DepartmentId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public DateOnly DateJoined { get; set; }
}