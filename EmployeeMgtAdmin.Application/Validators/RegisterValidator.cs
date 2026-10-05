using EmployeeMgtAdmin.Domain.Enums;
using EmployeeMgtAdmin.Application.DTOs.Auth;
using FluentValidation;

namespace EmployeeMgtAdmin.Application.Validators;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be valid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

        RuleFor(x => x.Role)
            .Must(r => r == UserRole.Admin || r == UserRole.Hr || r == UserRole.User)
            .WithMessage("Role must be Admin, HR, or User.");
    }
}