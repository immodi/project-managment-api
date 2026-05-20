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

public class LoginTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtProvider> _jwt = new();

    private readonly AuthService _service;

    public LoginTests()
    {
        _service = new AuthService(
            _users.Object,
            _hasher.Object,
            _jwt.Object
        );
    }

    [Fact]
    public async Task Should_return_token_when_credentials_are_valid()
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

        _jwt.Verify(x => x.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task Should_throw_unauthorized_when_user_does_not_exist()
    {
        var request = new LoginRequest
        {
            Email = "missing@test.com",
            Password = "Password123!"
        };

        _users.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(request));
    }

    [Fact]
    public async Task Should_throw_unauthorized_when_password_is_wrong()
    {
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "wrong-password"
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