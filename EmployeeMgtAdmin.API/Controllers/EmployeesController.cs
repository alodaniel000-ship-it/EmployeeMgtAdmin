using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EmployeeMgtAdmin.Application.DTOs.Common;
using EmployeeMgtAdmin.Application.DTOs.Employee;
using EmployeeMgtAdmin.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeMgtAdmin.API.Controllers;

[ApiController]
[Route("api/v1/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;
    private readonly IValidator<CreateEmployeeDto> _createValidator;
    private readonly IValidator<UpdateEmployeeDto> _updateValidator;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(
        EmployeeService employeeService,
        IValidator<CreateEmployeeDto> createValidator,
        IValidator<UpdateEmployeeDto> updateValidator,
        ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>Get paginated list of employees with optional filtering and sorting.</summary>
    [HttpGet]
    [Authorize(Policy = "AnyRole")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeResponseDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeQueryParams queryParams)
    {
        var result = await _employeeService.GetEmployeesAsync(queryParams);
        return Ok(ApiResponse<PagedResult<EmployeeResponseDto>>.Ok(result));
    }

    /// <summary>Get a single employee by ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AnyRole")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(employee));
    }

    /// <summary>Create a new employee.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOrHR")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 409)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse.Fail("Validation failed.", errors));
        }

        var created = await _employeeService.CreateAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<EmployeeResponseDto>.Ok(created, "Employee created successfully."));
    }

    /// <summary>Update an existing employee.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOrHR")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse.Fail("Validation failed.", errors));
        }

        var updated = await _employeeService.UpdateAsync(id, dto, GetCurrentUserId());
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(updated, "Employee updated successfully."));
    }

    /// <summary>Soft-delete an employee.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _employeeService.DeleteAsync(id, GetCurrentUserId());
        return Ok(ApiResponse.Ok("Employee deleted successfully."));
    }

    /// <summary>Activate an employee.</summary>
    [HttpPatch("{id:guid}/activate")]
    [Authorize(Policy = "AdminOrHR")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _employeeService.SetActiveStatusAsync(id, true, GetCurrentUserId());
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(result, "Employee activated."));
    }

    /// <summary>Deactivate an employee.</summary>
    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Policy = "AdminOrHR")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _employeeService.SetActiveStatusAsync(id, false, GetCurrentUserId());
        return Ok(ApiResponse<EmployeeResponseDto>.Ok(result, "Employee deactivated."));
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
