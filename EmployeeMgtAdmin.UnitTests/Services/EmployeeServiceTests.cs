using EmployeeMgtAdmin.Application.DTOs.Employee;
using EmployeeMgtAdmin.Application.Interfaces;
using EmployeeMgtAdmin.Application.Services;
using EmployeeMgtAdmin.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EmployeeManagement.UnitTests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IDepartmentRepository> _departmentRepoMock = new();
    private readonly Mock<IAuditLogRepository> _auditRepoMock = new();
    private readonly Mock<ILogger<EmployeeService>> _loggerMock = new();

    private EmployeeService CreateService() =>
        new(_employeeRepoMock.Object, _departmentRepoMock.Object, _auditRepoMock.Object, _loggerMock.Object);

    private static CreateEmployeeDto ValidCreateDto() => new()
    {
        EmployeeNumber = "EMP001",
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        DepartmentId = Guid.NewGuid(),
        JobTitle = "Software Engineer",
        Salary = 75000,
        DateOfBirth = new DateOnly(1990, 1, 1),
    };

    private static Employee SampleEmployee(Guid? id = null, Guid? deptId = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        EmployeeNumber = "EMP001",
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        DepartmentId = deptId ?? Guid.NewGuid(),
        JobTitle = "Software Engineer",
        Salary = 75000,
        DateOfBirth = new DateOnly(1990, 1, 1),
        IsActive = true,
        Department = new Department { Name = "Engineering", Code = "ENG" }
    };

    // ---- Create ----

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedEmployee()
    {
        var dto = ValidCreateDto();
        var employee = SampleEmployee(deptId: dto.DepartmentId);

        _employeeRepoMock.Setup(r => r.EmployeeNumberExistsAsync(dto.EmployeeNumber, null)).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.EmailExistsAsync(dto.Email, null)).ReturnsAsync(false);
        _departmentRepoMock.Setup(r => r.ExistsAsync(dto.DepartmentId)).ReturnsAsync(true);
        _employeeRepoMock.Setup(r => r.CreateAsync(It.IsAny<Employee>())).ReturnsAsync(employee);
        _auditRepoMock.Setup(r => r.LogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

        var service = CreateService();
        var result = await service.CreateAsync(dto, "admin");

        result.Should().NotBeNull();
        result.EmployeeNumber.Should().Be("EMP001");
        result.FirstName.Should().Be("John");
        _employeeRepoMock.Verify(r => r.CreateAsync(It.IsAny<Employee>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmployeeNumber_ThrowsInvalidOperationException()
    {
        var dto = ValidCreateDto();
        _employeeRepoMock.Setup(r => r.EmployeeNumberExistsAsync(dto.EmployeeNumber, null)).ReturnsAsync(true);

        var service = CreateService();
        await service.Invoking(s => s.CreateAsync(dto, "admin"))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*EMP001*already in use*");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        var dto = ValidCreateDto();
        _employeeRepoMock.Setup(r => r.EmployeeNumberExistsAsync(dto.EmployeeNumber, null)).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.EmailExistsAsync(dto.Email, null)).ReturnsAsync(true);

        var service = CreateService();
        await service.Invoking(s => s.CreateAsync(dto, "admin"))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already in use*");
    }

    [Fact]
    public async Task CreateAsync_WithInvalidDepartment_ThrowsKeyNotFoundException()
    {
        var dto = ValidCreateDto();
        _employeeRepoMock.Setup(r => r.EmployeeNumberExistsAsync(dto.EmployeeNumber, null)).ReturnsAsync(false);
        _employeeRepoMock.Setup(r => r.EmailExistsAsync(dto.Email, null)).ReturnsAsync(false);
        _departmentRepoMock.Setup(r => r.ExistsAsync(dto.DepartmentId)).ReturnsAsync(false);

        var service = CreateService();
        await service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Department*not found*");
    }

    // ---- GetById ----

    [Fact]
    public async Task GetByIdAsync_ExistingEmployee_ReturnsDto()
    {
        var emp = SampleEmployee();
        _employeeRepoMock.Setup(r => r.GetByIdAsync(emp.Id)).ReturnsAsync(emp);

        var service = CreateService();
        var result = await service.GetByIdAsync(emp.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(emp.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentEmployee_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _employeeRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        var service = CreateService();
        await service.Invoking(s => s.GetByIdAsync(id))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    // ---- Update ----

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedEmployee()
    {
        var id = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var emp = SampleEmployee(id, deptId);
        var dto = new UpdateEmployeeDto
        {
            FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com",
            DepartmentId = deptId, JobTitle = "Senior Engineer", Salary = 90000,
            DateOfBirth = new DateOnly(1990, 1, 1), DateJoined = new DateOnly(2022, 6, 1)
        };

        _employeeRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(emp);
        _employeeRepoMock.Setup(r => r.EmailExistsAsync(dto.Email, id)).ReturnsAsync(false);
        _departmentRepoMock.Setup(r => r.ExistsAsync(deptId)).ReturnsAsync(true);
        _employeeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>())).ReturnsAsync(emp);
        _auditRepoMock.Setup(r => r.LogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

        var service = CreateService();
        var result = await service.UpdateAsync(id, dto);

        result.Should().NotBeNull();
        _employeeRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Employee>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEmployee_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _employeeRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        var service = CreateService();
        await service.Invoking(s => s.UpdateAsync(id, new UpdateEmployeeDto()))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    // ---- Delete ----

    [Fact]
    public async Task DeleteAsync_ExistingEmployee_SoftDeletes()
    {
        var id = Guid.NewGuid();
        _employeeRepoMock.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);
        _employeeRepoMock.Setup(r => r.SoftDeleteAsync(id)).ReturnsAsync(true);
        _auditRepoMock.Setup(r => r.LogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

        var service = CreateService();
        await service.Invoking(s => s.DeleteAsync(id)).Should().NotThrowAsync();

        _employeeRepoMock.Verify(r => r.SoftDeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentEmployee_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _employeeRepoMock.Setup(r => r.ExistsAsync(id)).ReturnsAsync(false);

        var service = CreateService();
        await service.Invoking(s => s.DeleteAsync(id))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    // ---- Activate / Deactivate ----

    [Fact]
    public async Task SetActiveStatusAsync_Deactivate_SetsIsActiveFalse()
    {
        var emp = SampleEmployee();
        emp.IsActive = true;
        _employeeRepoMock.Setup(r => r.GetByIdAsync(emp.Id)).ReturnsAsync(emp);
        _employeeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>())).ReturnsAsync(emp);
        _auditRepoMock.Setup(r => r.LogAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

        var service = CreateService();
        await service.SetActiveStatusAsync(emp.Id, false);

        _employeeRepoMock.Verify(r => r.UpdateAsync(It.Is<Employee>(e => !e.IsActive)), Times.Once);
    }

    // ---- Pagination ----

    [Fact]
    public async Task GetEmployeesAsync_ReturnsPaginatedResult()
    {
        var employees = new List<Employee> { SampleEmployee(), SampleEmployee() };
        var queryParams = new EmployeeQueryParams { PageNumber = 1, PageSize = 10 };

        _employeeRepoMock.Setup(r => r.GetPagedAsync(queryParams))
            .ReturnsAsync((employees, 2));

        var service = CreateService();
        var result = await service.GetEmployeesAsync(queryParams);

        result.TotalRecords.Should().Be(2);
        result.Data.Should().HaveCount(2);
        result.TotalPages.Should().Be(1);
    }
}
