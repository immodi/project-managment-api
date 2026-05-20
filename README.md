# Project Management API

A scalable backend API built with ASP.NET Core Web API using Clean Architecture principles.

---

# Tech Stack

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- Docker & Docker Compose
- xUnit

---

# Architecture

The project follows Clean Architecture and is divided into the following layers:

- API
- Application
- Domain
- Infrastructure

Key practices used:

- Dependency Injection
- SOLID Principles
- DTOs
- Global Exception Handling
- Validation
- Repository Pattern

---

# Features

## Authentication

- Register
- Login
- JWT Authentication

## Projects

- Create Project
- Get All Projects
- Get Project By Id
- Update Project
- Delete Project

## Tasks

- Create Task
- Update Task Status
- Get Tasks By Project
- Delete Task

---

# Running the Project

## Option 1 — Docker (Recommended)

### Run Containers

```bash
docker-compose up --build
```

Docker Compose automatically provides all required environment variables.

---

## API URL

```txt
http://localhost:5000
```

## Swagger Documentation

```txt
http://localhost:5000/swagger
```

---

# Option 2 — Local Development

## Requirements

- .NET 9 SDK
- SQL Server

---

## Set Environment Variables

### Linux/macOS

```bash
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=ProjectManagementDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True"

export Jwt__Key="THIS_IS_A_SUPER_SECRET_KEY_CHANGE_IT"
export Jwt__Issuer="ProjectManagement"
export Jwt__Audience="ProjectManagementUsers"
export Jwt__ExpiryMinutes="60"
```

### Windows PowerShell

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=ProjectManagementDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True"

$env:Jwt__Key="THIS_IS_A_SUPER_SECRET_KEY_CHANGE_IT"
$env:Jwt__Issuer="ProjectManagement"
$env:Jwt__Audience="ProjectManagementUsers"
$env:Jwt__ExpiryMinutes="60"
```

---

# Apply Database Migrations

```bash
dotnet ef database update --project ProjectManagement/src/Infrastructure --startup-project ProjectManagement/src/ProjectManagement.Api
```

---

# Run the API

```bash
dotnet run --project ProjectManagement/src/ProjectManagement.Api
```

---

# Running Tests

```bash
dotnet test ProjectManagement/tests/Tests
```

---

# API Documentation

Swagger/OpenAPI documentation is available at:

```txt
/swagger
```

---

# Authentication

The API uses JWT Bearer Authentication.

### Steps

1. Register a user
2. Login to receive JWT token
3. Click "Authorize" in Swagger
4. Enter token as:

```txt
Bearer YOUR_TOKEN
```

---

# Database Migrations

Database migration files are included in the repository.

---

# Notes

- Environment variables are used for sensitive configuration.
- Swagger is enabled for API testing and documentation.
- The project is structured for scalability and maintainability.
- Docker support is included for easier setup and execution.
