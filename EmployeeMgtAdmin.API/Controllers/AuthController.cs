using EmployeeMgtAdmin.Application.DTOs.Auth.;
using EmployeeMgtAdmin.Application.DTOs.Common;
using EmployeeMgtAdmin.Application.Interfaces;
using EmployeeMgtAdmin.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeMgtAdmin.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterDto> _registerValidator;

    public AuthController(IAuthService authService, IValidator<RegisterDto> registerValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
    }

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var validation = await _registerValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse.Fail("Validation failed.", errors));
        }

        var result = await _authService.RegisterAsync(dto);
        return CreatedAtAction(nameof(Register), ApiResponse<AuthResponseDto>.Ok(result, "Account created successfully."));
    }

    /// <summary>Login and receive a JWT token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
    }
}