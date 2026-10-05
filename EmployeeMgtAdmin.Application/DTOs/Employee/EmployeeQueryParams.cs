namespace EmployeeMgtAdmin.Application.DTOs.Employee;

public class EmployeeQueryParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }           // searches name, email, employee number
    public string? Department { get; set; }        // filter by department name or code
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "lastName"; // name, salary, dateJoined, department
    public string? SortOrder { get; set; } = "asc";
}