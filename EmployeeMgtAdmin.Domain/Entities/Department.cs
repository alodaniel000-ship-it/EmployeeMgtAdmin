namespace EmployeeMgtAdmin.Domain.Entities;

public class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } =  string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } =  true;
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } =  DateTime.UtcNow;
    // Navigation
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}