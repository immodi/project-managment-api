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

public class RegisterTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtProvider> _jwt = new();

    private readonly AuthService _service;

    public RegisterTests()
    {
        _service = new AuthService(
            _users.Object,
            _hasher.Object,
            _jwt.Object
        );
    }

    [Fact]
    public async Task Should_create_user_and_return_token_when_email_is_new()
    {
        var request = new RegisterRequest
        {
            Email = "new@test.com",
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
    public async Task Should_throw_conflict_when_email_already_exists()
    {
        var request = new RegisterRequest
        {
            Email = "existing@test.com",
            Password = "Password123!"
        };

        _users.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = "existing-hash"
            });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RegisterAsync(request));
    }

    [Fact]
    public async Task Should_hash_password_before_saving_user()
    {
        var request = new RegisterRequest
        {
            Email = "hash@test.com",
            Password = "Password123!"
        };

        _users.Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _hasher.Setup(x => x.Hash(request.Password))
            .Returns("hashed-password");

        _jwt.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("jwt-token");

        await _service.RegisterAsync(request);

        _users.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.PasswordHash == "hashed-password"
        )), Times.Once);
    }
}