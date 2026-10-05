using EmployeeMgtAdmin.Application.DTOs.Employee;
using FluentValidation;



namespace EmployeeMgtAdmin.Application.Validators;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
{
public CreateEmployeeValidator()
{
    RuleFor(x => x.EmployeeNumber)
        .NotEmpty().WithMessage("Employee number is required.")
        .MaximumLength(20).WithMessage("Employee number must not exceed 20 characters.");

    RuleFor(x => x.FirstName)
        .NotEmpty().WithMessage("First name is required.")
        .MaximumLength(100);

    RuleFor(x => x.LastName)
        .NotEmpty().WithMessage("Last name is required.")
        .MaximumLength(100);

    RuleFor(x => x.Email)
        .NotEmpty().WithMessage("Email is required.")
        .EmailAddress().WithMessage("Email must be a valid email address.");

    RuleFor(x => x.DepartmentId)
        .NotEmpty().WithMessage("Department is required.");

    RuleFor(x => x.JobTitle)
        .NotEmpty().WithMessage("Job title is required.")
        .MaximumLength(100);

    RuleFor(x => x.Salary)
        .GreaterThan(0).WithMessage("Salary must be greater than zero.");

    RuleFor(x => x.DateOfBirth)
        .Must(dob => dob < DateOnly.FromDateTime(DateTime.UtcNow))
        .WithMessage("Date of birth cannot be in the future.");

    RuleFor(x => x.DateJoined)
        .Must((dto, dateJoined) => dateJoined >= dto.DateOfBirth)
        .WithMessage("Date joined cannot be earlier than date of birth.");
}
}