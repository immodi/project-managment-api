using API.Extensions;
using API.Middlewares;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Presentation
builder.Services.AddPresentation();

// Swagger
builder.Services.AddSwaggerDocumentation();

// JWT
var jwt = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionMiddleware>();

// Swagger
app.UseSwaggerDocumentation();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    const int retries = 10;
    var delay = TimeSpan.FromSeconds(5);

    for (var i = 0; i < retries; i++)
    {
        try
        {
            dbContext.Database.Migrate();
            break;
        }
        catch
        {
            if (i == retries - 1)
                throw;

            Thread.Sleep(delay);
        }
    }
}

app.Run();

public partial class Program { }