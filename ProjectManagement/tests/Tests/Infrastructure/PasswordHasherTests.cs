using Application.Interfaces;
using FluentAssertions;
using Infrastructure.Authentication;
using Tests.Common;
using Xunit;

namespace Tests.Infrastructure;

public class PasswordHasherTests : TestBase
{
    private readonly IPasswordHasher _hasher = new PasswordHasher();

    [Fact]
    public void Hash_ShouldReturnDifferentValueThanInput()
    {
        const string password = "TestPassword123!";

        var hash = _hasher.Hash(password);

        hash.Should().NotBe(password);
        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Verify_ShouldReturnTrue_ForCorrectPassword()
    {
        const string password = "TestPassword123!";
        var hash = _hasher.Hash(password);

        var result = _hasher.Verify(password, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_ForIncorrectPassword()
    {
        const string password = "TestPassword123!";
        const string wrongPassword = "WrongPassword!";
        var hash = _hasher.Hash(password);

        var result = _hasher.Verify(wrongPassword, hash);

        result.Should().BeFalse();
    }
}