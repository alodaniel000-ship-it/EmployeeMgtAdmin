using EmployeeMgtAdmin.Application.DTOs.Common;
using EmployeeMgtAdmin.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EmployeeMgtAdmin.API.Controllers;

[ApiController]
[Route("api/v1/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentRepository _departmentRepo;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "departments_active";

    public DepartmentsController(IDepartmentRepository departmentRepo, IMemoryCache cache)
    {
        _departmentRepo = departmentRepo;
        _cache = cache;
    }

    /// <summary>Get all active departments (cached for 5 minutes).</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll()
    {
        if (!_cache.TryGetValue(CacheKey, out var departments))
        {
            departments = await _departmentRepo.GetAllActiveAsync();
            _cache.Set(CacheKey, departments, TimeSpan.FromMinutes(5));
        }

        return Ok(ApiResponse<object>.Ok(departments!));
    }
}