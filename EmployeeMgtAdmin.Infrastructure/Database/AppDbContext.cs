using EmployeeMgtAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeMgtAdmin.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Department
        modelBuilder.Entity<Department>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).IsRequired().HasMaxLength(100);
            e.Property(d => d.Code).IsRequired().HasMaxLength(20);
            e.HasIndex(d => d.Code).IsUnique();
            e.Property(d => d.IsActive).HasDefaultValue(true);
            e.Property(d => d.CreatedAt).HasDefaultValueSql("now()");
            e.Property(d => d.UpdatedAt).HasDefaultValueSql("now()");
        });

        // Employee
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EmployeeNumber).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.EmployeeNumber).IsUnique();
            e.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            e.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.JobTitle).IsRequired().HasMaxLength(100);
            e.Property(x => x.Salary).HasColumnType("numeric(18,2)");
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.IsDeleted).HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

            e.HasOne(x => x.Department)
             .WithMany(d => d.Employees)
             .HasForeignKey(x => x.DepartmentId)
             .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete
            e.HasQueryFilter(x => !x.IsDeleted);

            // Indexes for commonly searched fields
            e.HasIndex(x => x.DepartmentId);
            e.HasIndex(x => x.IsActive);
            e.HasIndex(x => x.IsDeleted);
        });

        // AppUser
        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Username).IsRequired().HasMaxLength(50);
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Role).IsRequired().HasMaxLength(20);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EntityName).IsRequired().HasMaxLength(100);
            e.Property(x => x.EntityId).IsRequired().HasMaxLength(100);
            e.Property(x => x.Action).IsRequired().HasMaxLength(50);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var departments = new[]
        {
            new Department { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Engineering", Code = "ENG", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Human Resources", Code = "HR", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Finance", Code = "FIN", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Information Technology", Code = "IT", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
        };

        modelBuilder.Entity<Department>().HasData(departments);
    }
}
