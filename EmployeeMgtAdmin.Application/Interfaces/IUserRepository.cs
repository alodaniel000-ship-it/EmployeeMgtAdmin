using EmployeeMgtAdmin.Domain.Entities;

namespace EmployeeMgtAdmin.Application.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<AppUser> CreateAsync(AppUser user);
}