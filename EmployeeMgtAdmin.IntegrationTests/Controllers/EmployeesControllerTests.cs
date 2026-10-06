using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EmployeeMgtAdmin.Application.DTOs.Auth;
using EmployeeMgtAdmin.Application.DTOs.Common;
using EmployeeMgtAdmin.Application.DTOs.Employee;
using EmployeeMgtAdmin.IntegrationTests.Fixtures;
using FluentAssertions;
using Xunit;

namespace EmployeeMgtAdmin.IntegrationTests.Controllers;

public class EmployeesControllerTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly Guid DeptId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EmployeesControllerTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        // Register an admin user
        var register = new RegisterDto
        {
            Username = $"admin_{Guid.NewGuid():N}",
            Email = $"admin_{Guid.NewGuid():N}@test.com",
            Password = "Admin123!",
            Role = "Admin"
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", register);

        var login = new LoginDto { Email = register.Email, Password = register.Password };
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/login", login);
        var content = await resp.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(content, JsonOptions);
        return result!.Data!.Token;
    }

    private async Task<string> GetUserTokenAsync()
    {
        var register = new RegisterDto
        {
            Username = $"user_{Guid.NewGuid():N}",
            Email = $"user_{Guid.NewGuid():N}@test.com",
            Password = "User1234!",
            Role = "User"
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", register);

        var login = new LoginDto { Email = register.Email, Password = register.Password };
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/login", login);
        var content = await resp.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(content, JsonOptions);
        return result!.Data!.Token;
    }

    private static CreateEmployeeDto ValidEmployee(string suffix = "") => new()
    {
        EmployeeNumber = $"EMP{suffix}{Guid.NewGuid():N[..6]}",
        FirstName = "Test",
        LastName = "Employee",
        Email = $"test.{Guid.NewGuid():N}@company.com",
        DepartmentId = DeptId,
        JobTitle = "Developer",
        Salary = 60000,
        DateOfBirth = new DateOnly(1990, 1, 1),
       
    };

    [Fact]
    public async Task GetEmployees_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/employees");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateEmployee_AsAdmin_Returns201()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dto = ValidEmployee();
        var response = await _client.PostAsJsonAsync("/api/v1/employees", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateEmployee_AsUser_Returns403()
    {
        var token = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/employees", ValidEmployee());
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateEmployee_WithInvalidEmail_Returns400()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dto = ValidEmployee();
        dto.Email = "not-an-email";
        var response = await _client.PostAsJsonAsync("/api/v1/employees", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEmployee_WithDuplicateNumber_Returns409()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dto = ValidEmployee();
        await _client.PostAsJsonAsync("/api/v1/employees", dto);

        // Second employee with same number but different email
        var dto2 = ValidEmployee();
        dto2.EmployeeNumber = dto.EmployeeNumber;
        var response = await _client.PostAsJsonAsync("/api/v1/employees", dto2);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetEmployeeById_NotFound_Returns404()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/v1/employees/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEmployees_WithPagination_ReturnsPaged()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/employees?pageNumber=1&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<EmployeeResponseDto>>>(content, JsonOptions);
        result!.Data!.PageSize.Should().Be(5);
        result.Data.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task DeleteEmployee_AsAdmin_Returns200()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dto = ValidEmployee();
        var createResp = await _client.PostAsJsonAsync("/api/v1/employees", dto);
        var createContent = await createResp.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ApiResponse<EmployeeResponseDto>>(createContent, JsonOptions);

        var deleteResp = await _client.DeleteAsync($"/api/v1/employees/{created!.Data!.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteEmployee_AsUser_Returns403()
    {
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var dto = ValidEmployee();
        var createResp = await _client.PostAsJsonAsync("/api/v1/employees", dto);
        var createContent = await createResp.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ApiResponse<EmployeeResponseDto>>(createContent, JsonOptions);

        var userToken = await GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var deleteResp = await _client.DeleteAsync($"/api/v1/employees/{created!.Data!.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
