namespace EmployeeMgtAdmin.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; }  = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid DepartmentId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public bool IsActive { get; set; } =  true;
    public bool IsDeleted { get; set; } =  false; /*This is "soft delete". Instead of permanently removing
    records from the database, we just flip this flag to true. The employee still exists in the database but is hidden
    from all queries. This preserves audit history and allows recovery.*/
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } =  DateTime.UtcNow;
    
    //Navigation
    public Department? Department { get; set; }
}