using EmployeeMgtAdmin.Domain.Entities;

namespace EmployeeMgtAdmin.Application.Interfaces;

public interface IAuditLogRepository
{
    Task LogAsync(AuditLog log);
}