using Application.Common.Exceptions;
using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtProvider> _jwt = new();

    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(
            _users.Object,
            _hasher.Object,
            _jwt.Object
        );
    }

    [Fact]
    public async Task Register_Should_Create_User_And_Return_Token()
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        };

        _users.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _hasher.Setup(x => x.Hash(request.Password))
            .Returns("hashed-password");

        _jwt.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("jwt-token");

        var result = await _service.RegisterAsync(request);

        result.Token.Should().Be("jwt-token");

        _users.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.Email == request.Email &&
            u.PasswordHash == "hashed-password"
        )), Times.Once);
    }

    [Fact]
    public async Task Register_Should_Throw_Conflict_When_Email_Exists()
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        };

        _users.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = "existing"
            });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RegisterAsync(request));
    }

    [Fact]
    public async Task Login_Should_Return_Token_When_Credentials_Are_Valid()
    {
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = "hashed-password"
        };

        _users.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _hasher.Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        _jwt.Setup(x => x.GenerateToken(user))
            .Returns("jwt-token");

        var result = await _service.LoginAsync(request);

        result.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task Login_Should_Throw_Unauthorized_When_Invalid_Credentials()
    {
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "wrong"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = "hashed-password"
        };

        _users.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _hasher.Setup(x => x.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(request));
    }
}