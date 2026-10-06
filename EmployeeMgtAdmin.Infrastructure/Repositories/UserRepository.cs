using EmployeeMgtAdmin.Application.Interfaces;
using EmployeeMgtAdmin.Domain.Entities;
using EmployeeMgtAdmin.Infrastructure.Database;
using EmployeeMgtAdmin.Infrastructure.Database.Migrations;
using Microsoft.EntityFrameworkCore;

namespace EmployeeMgtAdmin.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public async Task<AppUser?> GetByEmailAsync(string email) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> EmailExistsAsync(string email) =>
        await _db.Users.AnyAsync(u => u.Email == email);

    public async Task<AppUser> CreateAsync(AppUser user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}