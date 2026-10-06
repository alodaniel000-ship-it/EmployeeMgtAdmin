using EmployeeMgtAdmin.API.Extensions;
using EmployeeMgtAdmin.API.Middleware;
using EmployeeMgtAdmin.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithAuth();
builder.Services.AddRateLimiting();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Management API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Only redirect to HTTPS in non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("fixed");
app.MapHealthChecks("/health");

app.Run();

// Partial class needed for integration test WebApplicationFactory
public partial class Program { }

//run ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/EmployeeManagement.API  in terminal to begin
//Then open http://localhost:5000 in your browser.