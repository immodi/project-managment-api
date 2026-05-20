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

## Option 1 — Docker Compose (Recommended)

### Start Services

```bash
docker compose up --build
```

The API will be available at:

```txt
http://localhost:5000
```

Swagger:

```txt
http://localhost:5000/
```

---

# Option 2 — Local Development

## Requirements

- .NET 9 SDK
- Docker

---

# Clone Repository

```bash
git clone https://github.com/immodi/project-managment-api.git

cd project-managment-api
```

---

# Start SQL Server Container

Remove old container if it already exists:

```bash
docker rm -f projectmanagement-sql
```

Run SQL Server:

```bash
docker run \
-e "ACCEPT_EULA=Y" \
-e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
-p 1433:1433 \
--name projectmanagement-sql \
-d mcr.microsoft.com/mssql/server:2022-latest
```

Wait until SQL Server is ready:

```bash
docker logs -f projectmanagement-sql
```

You should see:

```txt
SQL Server is now ready for client connections
```

---

# Restore Packages

```bash
dotnet restore
```

---

# Environment Variables

## Linux/macOS

```bash
export ASPNETCORE_ENVIRONMENT="Development"

export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=ProjectManagementDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Encrypt=False"

export Jwt__Key="THIS_IS_A_SUPER_SECRET_KEY_CHANGE_IT"
export Jwt__Issuer="ProjectManagement"
export Jwt__Audience="ProjectManagementUsers"
export Jwt__ExpiryMinutes="60"
```

## Windows PowerShell

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"

$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=ProjectManagementDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Encrypt=False"

$env:Jwt__Key="THIS_IS_A_SUPER_SECRET_KEY_CHANGE_IT"
$env:Jwt__Issuer="ProjectManagement"
$env:Jwt__Audience="ProjectManagementUsers"
$env:Jwt__ExpiryMinutes="60"
```

---

# Run the API

```bash
dotnet run --launch-profile http --project ProjectManagement/src/API
```

The application automatically applies database migrations on startup.

The API will run on a local development port similar to:

```txt
http://localhost:5094
```

Swagger documentation:

```txt
http://localhost:5094/
```

---

# Running Tests

```bash
dotnet run --project ProjectManagement/tests/Tests
```

# Authentication

The API uses JWT Bearer Authentication.

## Steps

1. Register a user
2. Login to receive JWT token
3. Click "Authorize" in Swagger
4. Enter token as:

```txt
Bearer YOUR_TOKEN
```

---

# Notes

- Database migrations are automatically applied during application startup.
- Reusing old Docker volumes can cause SQL Server login issues if passwords change.
- Swagger is enabled for API testing and documentation.
- Docker support is included for easier setup and execution.
