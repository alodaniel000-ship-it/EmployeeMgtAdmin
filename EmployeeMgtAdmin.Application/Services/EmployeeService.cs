using EmployeeMgtAdmin.Application.DTOs.Common;
using EmployeeMgtAdmin.Application.DTOs.Employee;
using EmployeeManagement.Application.Interfaces;
using EmployeeMgtAdmin.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using EmployeeMgtAdmin.Application.Interfaces;


namespace EmployeeMgtAdmin.Application.Services;

public class EmployeeService
{
     private readonly IEmployeeRepository _employeeRepo;
    private readonly IDepartmentRepository _departmentRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository employeeRepo,
        IDepartmentRepository departmentRepo,
        IAuditLogRepository auditRepo,
        ILogger<EmployeeService> logger)
    {
        _employeeRepo = employeeRepo;
        _departmentRepo = departmentRepo;
        _auditRepo = auditRepo;
        _logger = logger;
    }

    public async Task<PagedResult<EmployeeResponseDto>> GetEmployeesAsync(EmployeeQueryParams queryParams)
    {
        var (items, total) = await _employeeRepo.GetPagedAsync(queryParams);

        return new PagedResult<EmployeeResponseDto>
        {
            Data = items.Select(MapToResponseDto),
            PageNumber = queryParams.PageNumber,
            PageSize = queryParams.PageSize,
            TotalRecords = total
        };
    }

    public async Task<EmployeeResponseDto> GetByIdAsync(Guid id)
    {
        var employee = await _employeeRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found.");

        return MapToResponseDto(employee);
    }

    public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto, string? performedBy = null)
    {
        if (await _employeeRepo.EmployeeNumberExistsAsync(dto.EmployeeNumber))
            throw new InvalidOperationException($"Employee number '{dto.EmployeeNumber}' is already in use.");

        if (await _employeeRepo.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException($"Email '{dto.Email}' is already in use.");

        if (!await _departmentRepo.ExistsAsync(dto.DepartmentId))
            throw new KeyNotFoundException($"Department with ID {dto.DepartmentId} not found.");

        var employee = new Employee
        {
            EmployeeNumber = dto.EmployeeNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.PhoneNumber,
            DepartmentId = dto.DepartmentId,
            JobTitle = dto.JobTitle,
            Salary = dto.Salary,
            DateOfBirth = dto.DateOfBirth,
        };

        var created = await _employeeRepo.CreateAsync(employee);

        _logger.LogInformation("Employee {EmployeeId} ({EmployeeNumber}) created by {UserId}",
            created.Id, created.EmployeeNumber, performedBy ?? "system");

        await _auditRepo.LogAsync(new AuditLog
        {
            EntityName = "Employee",
            EntityId = created.Id.ToString(),
            Action = "Created",
            PerformedBy = performedBy,
            NewValues = JsonSerializer.Serialize(new { created.FirstName, created.LastName, created.Email })
        });

        return MapToResponseDto(created);
    }

    public async Task<EmployeeResponseDto> UpdateAsync(Guid id, UpdateEmployeeDto dto, string? performedBy = null)
    {
        var employee = await _employeeRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found.");

        if (await _employeeRepo.EmailExistsAsync(dto.Email, id))
            throw new InvalidOperationException($"Email '{dto.Email}' is already in use.");

        if (!await _departmentRepo.ExistsAsync(dto.DepartmentId))
            throw new KeyNotFoundException($"Department with ID {dto.DepartmentId} not found.");

        var oldValues = JsonSerializer.Serialize(new { employee.FirstName, employee.LastName, employee.Email, employee.Salary });

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Phone = dto.PhoneNumber;
        employee.DepartmentId = dto.DepartmentId;
        employee.JobTitle = dto.JobTitle;
        employee.Salary = dto.Salary;
        employee.DateOfBirth = dto.DateOfBirth;
        employee.UpdatedAt = DateTime.UtcNow;

        var updated = await _employeeRepo.UpdateAsync(employee);

        _logger.LogInformation("Employee {EmployeeId} updated by {UserId}", id, performedBy ?? "system");

        await _auditRepo.LogAsync(new AuditLog
        {
            EntityName = "Employee",
            EntityId = id.ToString(),
            Action = "Updated",
            PerformedBy = performedBy,
            OldValues = oldValues,
            NewValues = JsonSerializer.Serialize(new { updated.FirstName, updated.LastName, updated.Email, updated.Salary })
        });

        return MapToResponseDto(updated);
    }

    public async Task DeleteAsync(Guid id, string? performedBy = null)
    {
        if (!await _employeeRepo.ExistsAsync(id))
            throw new KeyNotFoundException($"Employee with ID {id} not found.");

        await _employeeRepo.SoftDeleteAsync(id);

        _logger.LogInformation("Employee {EmployeeId} soft-deleted by {UserId}", id, performedBy ?? "system");

        await _auditRepo.LogAsync(new AuditLog
        {
            EntityName = "Employee",
            EntityId = id.ToString(),
            Action = "Deleted",
            PerformedBy = performedBy
        });
    }

    public async Task<EmployeeResponseDto> SetActiveStatusAsync(Guid id, bool isActive, string? performedBy = null)
    {
        var employee = await _employeeRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Employee with ID {id} not found.");

        employee.IsActive = isActive;
        employee.UpdatedAt = DateTime.UtcNow;

        var updated = await _employeeRepo.UpdateAsync(employee);

        var action = isActive ? "Activated" : "Deactivated";
        _logger.LogInformation("Employee {EmployeeId} {Action} by {UserId}", id, action, performedBy ?? "system");

        await _auditRepo.LogAsync(new AuditLog
        {
            EntityName = "Employee",
            EntityId = id.ToString(),
            Action = action,
            PerformedBy = performedBy
        });

        return MapToResponseDto(updated);
    }

    private static EmployeeResponseDto MapToResponseDto(Employee e) => new()
    {
        Id = e.Id,
        EmployeeNumber = e.EmployeeNumber,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email,
        PhoneNumber = e.Phone,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department?.Name ?? string.Empty,
        JobTitle = e.JobTitle,
        Salary = e.Salary,
        DateOfBirth = e.DateOfBirth,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}