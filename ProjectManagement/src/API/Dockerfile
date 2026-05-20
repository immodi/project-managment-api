FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore ProjectManagement/src/ProjectManagement.Api/ProjectManagement.Api.csproj

RUN dotnet publish ProjectManagement/src/ProjectManagement.Api/ProjectManagement.Api.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ProjectManagement.Api.dll"]