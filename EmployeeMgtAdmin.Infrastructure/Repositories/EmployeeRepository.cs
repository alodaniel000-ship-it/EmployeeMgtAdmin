using EmployeeMgtAdmin.Application.Interfaces;
using EmployeeMgtAdmin.Application.DTOs.Employee;
using EmployeeMgtAdmin.Application.Interfaces;
using EmployeeMgtAdmin.Domain.Entities;
using EmployeeMgtAdmin.Infrastructure.Database;
using EmployeeMgtAdmin.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace EmployeeMgtAdmin.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;

    public EmployeeRepository(AppDbContext db) => _db = db;

    public async Task<Employee?> GetByIdAsync(Guid id) =>
        await _db.Employees.Include(e => e.Department).FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber) =>
        await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber);

    public async Task<Employee?> GetByEmailAsync(string email) =>
        await _db.Employees.FirstOrDefaultAsync(e => e.Email == email);

    public async Task<(IEnumerable<Employee> Items, int TotalCount)> GetPagedAsync(EmployeeQueryParams q)
    {
        var query = _db.Employees.Include(e => e.Department).AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(search) ||
                e.LastName.ToLower().Contains(search) ||
                e.Email.ToLower().Contains(search) ||
                e.EmployeeNumber.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(q.Department))
        {
            var dept = q.Department.ToLower();
            query = query.Where(e =>
                e.Department != null &&
                (e.Department.Name.ToLower().Contains(dept) ||
                 e.Department.Code.ToLower().Contains(dept)));
        }

        if (q.IsActive.HasValue)
            query = query.Where(e => e.IsActive == q.IsActive.Value);

        var total = await query.CountAsync();

        // Sorting
        query = (q.SortBy?.ToLower(), q.SortOrder?.ToLower()) switch
        {
            ("firstname" or "name", "desc") => query.OrderByDescending(e => e.FirstName),
            ("firstname" or "name", _)      => query.OrderBy(e => e.FirstName),
            ("salary", "desc")              => query.OrderByDescending(e => e.Salary),
            ("salary", _)                   => query.OrderBy(e => e.Salary),
            ("datejoined", "desc")          => query.OrderByDescending(e => e.DateJoined),
            ("datejoined", _)               => query.OrderBy(e => e.DateJoined),
            ("department", "desc")          => query.OrderByDescending(e => e.Department!.Name),
            ("department", _)               => query.OrderBy(e => e.Department!.Name),
            (_, "desc")                     => query.OrderByDescending(e => e.LastName),
            _                               => query.OrderBy(e => e.LastName)
        };

        var items = await query
            .Skip((q.PageNumber - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();
        // reload with navigation
        return (await GetByIdAsync(employee.Id))!;
    }

    public async Task<Employee> UpdateAsync(Employee employee)
    {
        _db.Employees.Update(employee);
        await _db.SaveChangesAsync();
        return (await GetByIdAsync(employee.Id))!;
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null) return false;

        employee.IsDeleted = true;
        employee.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id) =>
        await _db.Employees.AnyAsync(e => e.Id == id);

    public async Task<bool> EmployeeNumberExistsAsync(string employeeNumber, Guid? excludeId = null) =>
        await _db.Employees.AnyAsync(e =>
            e.EmployeeNumber == employeeNumber && (excludeId == null || e.Id != excludeId));

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null) =>
        await _db.Employees.AnyAsync(e =>
            e.Email == email && (excludeId == null || e.Id != excludeId));
}
