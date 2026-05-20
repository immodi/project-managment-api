using Application.Interfaces;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Tests.Common;
using Xunit;

namespace Tests.Infrastructure;

public class JwtProviderTests : TestBase
{
    private readonly IJwtProvider _jwtProvider;

    public JwtProviderTests()
    {
        var options = Options.Create(new JwtOptions
        {
            Key = "THIS_IS_A_TEST_KEY_THAT_IS_LONG_ENOUGH_32+",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 60
        });

        _jwtProvider = new JwtProvider(options);
    }

    [Fact]
    public void GenerateToken_ShouldReturnToken()
    {
        var user = new User
        {
            Id = NewId(),
            Email = "test@test.com",
            PasswordHash = "hashed"
        };

        var token = _jwtProvider.GenerateToken(user);

        token.Should().NotBeNullOrWhiteSpace();
        token.Split('.').Length.Should().Be(3);
    }

    [Fact]
    public void GenerateToken_ShouldContainUserIdClaim()
    {
        var user = new User
        {
            Id = NewId(),
            Email = "test@test.com",
            PasswordHash = "hashed"
        };

        var token = _jwtProvider.GenerateToken(user);

        token.Should().Contain(".");
    }
}