using EmployeeMgtAdmin.Application.DTOs.Auth;
namespace EmployeeMgtAdmin.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}