using EmployeeMgtAdmin.Domain.Entities;

namespace EmployeeMgtAdmin.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(Guid id);
    Task<IEnumerable<Department>> GetAllActiveAsync();
    Task<bool> ExistsAsync(Guid id);
}