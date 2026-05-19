# Project Management API

A backend REST API for a simple project management system built with ASP.NET Core 9, Entity Framework Core, and SQL Server (Docker).

## Features
- JWT Authentication
- Projects CRUD
- Tasks CRUD
- FluentValidation
- Clean Architecture

## Setup

### Run SQL Server
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Str0ngPassyazi" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

### Run migrations
dotnet ef database update --project Infrastructure --startup-project API

### Run API
dotnet run --project API

## Endpoints
- /api/auth/register
- /api/auth/login
- /api/projects
- /api/tasks

