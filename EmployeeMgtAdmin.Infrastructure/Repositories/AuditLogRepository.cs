using EmployeeMgtAdmin.Application.Interfaces;
using EmployeeMgtAdmin.Domain.Entities;
using EmployeeMgtAdmin.Infrastructure.Database;
using EmployeeMgtAdmin.Infrastructure.Database;

namespace EmployeeMgtAdmin.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _db;

    public AuditLogRepository(AppDbContext db) => _db = db;

    public async Task LogAsync(AuditLog log)
    {
        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}