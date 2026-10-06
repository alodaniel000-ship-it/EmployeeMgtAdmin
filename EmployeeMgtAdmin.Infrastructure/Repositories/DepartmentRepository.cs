using EmployeeMgtAdmin.Application.Interfaces;
using EmployeeMgtAdmin.Domain.Entities;
using EmployeeMgtAdmin.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace EmployeeMgtAdmin.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _db;

    public DepartmentRepository(AppDbContext db) => _db = db;

    public async Task<Department?> GetByIdAsync(Guid id) =>
        await _db.Departments.FindAsync(id);

    public async Task<IEnumerable<Department>> GetAllActiveAsync() =>
        await _db.Departments.Where(d => d.IsActive).ToListAsync();

    public async Task<bool> ExistsAsync(Guid id) =>
        await _db.Departments.AnyAsync(d => d.Id == id && d.IsActive);
}
