# Employee Management API

A RESTful Web API built with .NET 8 and PostgreSQL for managing employee records. Supports JWT authentication, role-based authorization, pagination, filtering, sorting, audit logging, and soft delete.

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/) — or use Docker Compose
- [Docker](https://www.docker.com/) (optional, for containerized setup)

---

## Project Structure

```
EmployeeMgtAdmin.API          — Controllers, Middleware, Extensions, Program.cs
EmployeeMgtAdmin.Application  — DTOs, Services, Validators, Interfaces
EmployeeMgtAdmin.Domain       — Entities, Enums
EmployeeMgtAdmin.Infrastructure — Repositories, DbContext, AuthService, Migrations
```

---

## Configuration and Environment Variables

Edit `EmployeeMgtAdmin.API/appsettings.json` (or use environment variables):

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Jwt:Key` | Secret key (minimum 32 characters) |
| `Jwt:Issuer` | Token issuer |
| `Jwt:Audience` | Token audience |
| `Jwt:ExpiryHours` | Token lifetime in hours (default: 8) |

---

## Database Setup

### Option A — Local PostgreSQL

1. Create a database:
   ```sql
   CREATE DATABASE EmployeeMgtAdmin;
   ```
2. Update the connection string in `appsettings.Development.json`.

### Option B — Docker Compose

```bash
docker-compose up -d db
```

---

## Database Migration Instructions

Migrations are applied automatically on startup. To add a new migration manually:

```bash
dotnet ef migrations add <MigrationName> \
  --project EmployeeMgtAdmin.Infrastructure \
  --startup-project EmployeeMgtAdmin.API

dotnet ef database update \
  --project EmployeeMgtAdmin.Infrastructure \
  --startup-project EmployeeMgtAdmin.API
```

---

## How to Run the Application

### Locally

```bash
dotnet restore
dotnet run --project EmployeeMgtAdmin.API
```

The API will be available at `http://localhost:5000`.  
Swagger UI opens at `http://localhost:5000` (root).

### With Docker Compose

```bash
docker-compose up --build
```

API available at `http://localhost:8080`.

---

## API Endpoint Documentation

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/register` | Register a new user |
| POST | `/api/v1/auth/login` | Login and get a JWT token |

### Employees

All employee endpoints require a `Bearer` token.

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/employees` | Any | Get paginated employees |
| GET | `/api/v1/employees/{id}` | Any | Get employee by ID |
| POST | `/api/v1/employees` | Admin, HR | Create employee |
| PUT | `/api/v1/employees/{id}` | Admin, HR | Update employee |
| DELETE | `/api/v1/employees/{id}` | Admin | Soft-delete employee |
| PATCH | `/api/v1/employees/{id}/activate` | Admin, HR | Activate employee |
| PATCH | `/api/v1/employees/{id}/deactivate` | Admin, HR | Deactivate employee |

### Departments

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/departments` | Get all active departments (cached) |

### Other

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check |

---

## Pagination, Filtering, and Sorting

```
GET /api/v1/employees?pageNumber=1&pageSize=20
GET /api/v1/employees?department=IT&isActive=true&pageNumber=1&pageSize=10
GET /api/v1/employees?search=john&sortBy=salary&sortOrder=desc
```

Supported `sortBy` values: `firstName`, `salary`, `dateJoined`, `department` (default: `lastName`).

Example response:
```json
{
  "success": true,
  "data": {
    "data": [],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 35,
    "totalPages": 4
  }
}
```

---

## Authentication Instructions

1. Register a user:
```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","email":"admin@example.com","password":"Admin1234","role":"Admin"}'
```

2. Login to get a token:
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin1234"}'
```

3. Use the token in subsequent requests:
```bash
curl -H "Authorization: Bearer <token>" http://localhost:5000/api/v1/employees
```

Roles: `Admin`, `HR`, `User`

---

## Example Requests and Responses

### Create Employee
```bash
curl -X POST http://localhost:5000/api/v1/employees \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeNumber": "EMP001",
    "firstName": "Jane",
    "lastName": "Doe",
    "email": "jane.doe@example.com",
    "phoneNumber": "+1234567890",
    "departmentId": "44444444-4444-4444-4444-444444444444",
    "jobTitle": "Software Engineer",
    "salary": 75000,
    "dateOfBirth": "1990-05-15",
    "dateJoined": "2022-01-10"
  }'
```

Response:
```json
{
  "success": true,
  "message": "Employee created successfully.",
  "data": {
    "id": "...",
    "employeeNumber": "EMP001",
    "firstName": "Jane",
    "lastName": "Doe",
    "email": "jane.doe@example.com",
    "isActive": true
  }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Employee not found",
  "errors": []
}
```

---

## How to Run Tests

```bash
dotnet test
```

---

## Seeded Departments

The database is seeded with four departments on first migration:

| ID | Name | Code |
|----|------|------|
| `11111111-...` | Engineering | ENG |
| `22222222-...` | Human Resources | HR |
| `33333333-...` | Finance | FIN |
| `44444444-...` | Information Technology | IT |
