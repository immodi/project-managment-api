using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Application.Common.Exceptions;

namespace Application.Services;

public class AuthService(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtProvider jwt)
    : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var exists = await users.GetByEmailAsync(request.Email);
        if (exists != null)
            throw new ConflictException("Email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = hasher.Hash(request.Password)
        };

        await users.AddAsync(user);

        return new AuthResponse
        {
            Token = jwt.GenerateToken(user)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await users.GetByEmailAsync(request.Email);

        if (user == null || !hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials");

        return new AuthResponse
        {
            Token = jwt.GenerateToken(user)
        };
    }
}