using EmployeeMgtAdmin.Application.DTOs.Employee;
using EmployeeMgtAdmin.Application.Validators;
using FluentAssertions;
using Xunit;

namespace EmployeeMgtAdmin.UnitTests.Validators;

public class CreateEmployeeValidatorTests
{
    private readonly CreateEmployeeValidator _validator = new();

    private static CreateEmployeeDto ValidDto() => new()
    {
        EmployeeNumber = "EMP001",
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        DepartmentId = Guid.NewGuid(),
        JobTitle = "Developer",
        Salary = 50000,
        DateOfBirth = new DateOnly(1990, 5, 10),
    };

    [Fact]
    public async Task ValidDto_PassesValidation()
    {
        var result = await _validator.ValidateAsync(ValidDto());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task MissingFirstName_FailsValidation()
    {
        var dto = ValidDto();
        dto.FirstName = string.Empty;
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public async Task MissingLastName_FailsValidation()
    {
        var dto = ValidDto();
        dto.LastName = string.Empty;
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }

    [Fact]
    public async Task InvalidEmail_FailsValidation()
    {
        var dto = ValidDto();
        dto.Email = "not-an-email";
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task EmptyEmail_FailsValidation()
    {
        var dto = ValidDto();
        dto.Email = string.Empty;
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ZeroSalary_FailsValidation()
    {
        var dto = ValidDto();
        dto.Salary = 0;
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Salary");
    }

    [Fact]
    public async Task NegativeSalary_FailsValidation()
    {
        var dto = ValidDto();
        dto.Salary = -100;
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task FutureDateOfBirth_FailsValidation()
    {
        var dto = ValidDto();
        dto.DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DateOfBirth");
    }

    [Fact]
    public async Task DateJoinedBeforeDateOfBirth_FailsValidation()
    {
        var dto = ValidDto();
        dto.DateOfBirth = new DateOnly(1990, 5, 10);
        dto.DateJoined = new DateOnly(1985, 1, 1); // before birth
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DateJoined");
    }

    [Fact]
    public async Task EmptyEmployeeNumber_FailsValidation()
    {
        var dto = ValidDto();
        dto.EmployeeNumber = string.Empty;
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task EmptyDepartmentId_FailsValidation()
    {
        var dto = ValidDto();
        dto.DepartmentId = Guid.Empty;
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
    }
}
