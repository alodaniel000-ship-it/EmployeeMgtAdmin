using EmployeeMgtAdmin.Application.DTOs.Employee;
using EmployeeMgtAdmin.Domain.Entities;

namespace EmployeeManagement.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id);
    Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber);
    Task<Employee?> GetByEmailAsync(string email);
    Task<(IEnumerable<Employee> Items, int TotalCount)> GetPagedAsync(EmployeeQueryParams queryParams);
    Task<Employee> CreateAsync(Employee employee);
    Task<Employee> UpdateAsync(Employee employee);
    Task<bool> SoftDeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> EmployeeNumberExistsAsync(string employeeNumber, Guid? excludeId = null);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null);
}